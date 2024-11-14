using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentFTP;
using System.IO;
using Microsoft.Extensions.Logging;


namespace FunctionApp3
{
    public class FTPConnection
    {
        [FunctionName("FTPConnection")]
        /**
         * 
         * Last edited by: Jake van de Kolk
         * 
         * Edited on: 30/09/2024
         * 
         * Reason for edit: changed time to agreed on time.
         * 
         * Get SQL results en setup the timer
         * 
         * @param myTimer timer function
         * 
         * @param log logging tool
         * 
         * */

        public async Task Run([TimerTrigger("0 0 23 * * *")] TimerInfo myTimer, ILogger log)
        {
            // Get the connection string from app settings and use it to create a connection.
            string str = Environment.GetEnvironmentVariable("SQLConnectionString");
            if (str == null)
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(str))
            {
                //open sql connection
                conn.Open();
                //create SQL query
                string text = "SELECT TOP 20 FROM [dbo].[Invoice]";

                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    // Execute the command and get the SqlDataReader
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        // Create a list to store the resultset
                        List<string[]> result = new List<string[]>();

                        // Loop through the result
                        while (await reader.ReadAsync())
                        {
                            List<string> row_result = new List<string>();
                            //get sql data


                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row_result.Add(!reader.IsDBNull(i) ? reader.GetString(i) : "");
                            }


                            //add results to resultset
                            result.Add(row_result.ToArray());
                        }

                        conn.Close();

                        // Write results to CSV
                        CSVActions csvActions = new CSVActions();
                        csvActions.WriteCSV(result, log);
                        SFTPUpload(log);

                    }
                }
            }

        }

        /**
         * 
         * Last edited by: Jake van de kolk
         * 
         * Edited on: 30/09/2024
         * 
         * Reason for editing: made code safer(cross side scripting prevention)
         * 
         * upload CSV to SFTP link
         * 
         */
        public async void SFTPUpload(ILogger log)
        {
            try
            {
                // get secrets from local settings
                string username = Environment.GetEnvironmentVariable("SFTPUserName");
                string password = Environment.GetEnvironmentVariable("SFTPPassword");
                string host = Environment.GetEnvironmentVariable("SFTPHost");

                // connect to the SFTP server
                using (FtpClient client = new FtpClient(host, username, password))
                {
                    client.Connect();
                    if (!client.IsConnected) 
                    {
                        log.LogError("client isn't connected to SFTP");
                        return;
                    }
                    username = "";
                    password = "";
                    host = "";

                    // get paths
                    string localPath = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "D:\\home", "site", "wwwroot", "file.csv");
                    string remotePath = "/httpdocs/wp-content/uploads/woocommerce_uploads/HP_import.csv";

                    // upload file
                    client.UploadFile(localPath, remotePath);
                   
                    localPath = "";
                    remotePath = "";
                    //disconnect from FTP
                    client.Disconnect();

                    log.LogInformation("succesfully uploaded to SFTP");
                    //make sure the function doesn't keep repeating
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }   


            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return;
            }
        }
    }
}
