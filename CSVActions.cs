using CsvHelper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;


namespace FunctionApp3
{

    /**
     * 
     * class that transform Azure SQL data into woo-commerce conform CSV file 
     * 
     * 
     * **/
    public class CSVActions
    {

        //Date_Changed field from invoice table

        // All properties names are based of the woocommerce import options

        // all fields that are commented out are the remaining CSV fields for woocommerce might be of use for future realeses. Read the guidelines here: https://woocommerce.com/document/product-csv-importer-exporter/


        //Product_ID field from invoice table
        public string id { get; set; }
        //Serie field from database
        public string type { get; set; }
        //public string sku { get; set; }
        //Shockbreaker field from the invoice table
        public string name { get; set; }
        //status field from database
        //public bool status { get; set; }
        // public bool featured { get; set; }
        //public string catalog_visbility {  get; set; }
        // date_change field from database
        public string short_description { get; set; }
        // public string description { get; set; }
         public string regular_price { get; set; }
       /* public double sale_price { get; set; }
        public string date_on_sale_from {  get; set; }  
        public string date_on_sale_to {  get; set; }
        public string tax_status { get; set; }
        public string tax_class { get; set; }
        public bool stock_status { get; set; }
        public int stock_quantity {  get; set; }
        public int low_stock_amount { get; set; }
        public bool backorders {  get; set; }
        public bool sold_individually { get; set; }
        public double weight { get; set; }
        public double length { get; set; }
        public double width {  get; set; }
        public double height { get; set; }
        public bool reviews_allowed {  get; set; }
        public string purchase_note { get; set; }
        public string[] category_ids { get; set; }
        public string[] tag_ids { get; set; }
        public string shipping_class_id { get; set; }
        public string image_id { get; set; }
        public int download_limit { get; set; }
        public int download_expiry {  get; set; }
        public string parent_id { get; set; }
        public string children { get; set; }
        public string upsell_ids {  get; set; }
        public string cross_sell_ids { get; set; } 
        public string product_url { get; set; } */
        public string button_text { get; set; }
        /*
        public int menu_order {  get; set; }
        //attributes has the categories: 1 color, 
        public string[] attributes {  get; set; }
        public string default_attributes { get; set; }
        public string[] downloads { get; set; } */

        //skipped chassis_number don't know where it should be in a woo commerce conform csv

        /**
         * Last edited by: Jake van de kolk
         * 
         * Edited on: 30/09/2024
         * 
         * Reason for editing: added cross side scripting prevention
         * 
         * Create a CSV file
         * 
         * @param data List<string[]> sql data that needs to be put in a CSV
         */
        public void WriteCSV(List<string[]> data, ILogger log)
        {
            //create a list of CSVrows
            List<CSVActions> values = new List<CSVActions>();


            //add all sql items into the CSV list
            foreach (string[] item in data)
            {
                values.Add(new CSVActions() { id = item[0], button_text = item[1], short_description = item[2], name = item[3], type = item[4], regular_price = item[6] });
            }
            if(values == null)
            {
                log.LogError("no data to write in CSV");
                return;
            } 


            //path to write the CSV file
            string homePath = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "D:\\home", "site", "wwwroot", "file.csv");

            using (StreamWriter writer = new StreamWriter(homePath))
            using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                homePath = "";
                //write CSV header
                csv.WriteHeader<CSVActions>();
                csv.NextRecord();
                //write data
                foreach (CSVActions item in values)
                {
                    csv.WriteRecord(item);
                    csv.NextRecord();
                }
            }
            log.LogInformation("CSV succesfully writen");
        }



    }
}
