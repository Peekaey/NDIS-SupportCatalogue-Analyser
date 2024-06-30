using PricelistGenerator.Models;
using PricelistGenerator.Models.File;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using PricelistGenerator.Interfaces.Helpers;
using PricelistGenerator.Models.ExportAnalysisChanges;
using PricelistGenerator.Models.Menu;

namespace PricelistGenerator.Helpers;

public class CsvHelper: ICsvHelper
{
    public string ExportProdaPricelistToCsv(Pricelist pricelist, SpreadsheetFile spreadsheetFile, RegionMenuOptions selectedRegion)
    {
        PricelistHelper pricelistHelper = new PricelistHelper();
        try
        {
            var regionName = pricelistHelper.GetRegionDescription(selectedRegion);
            var fileName = "Standard - " + regionName + " - " + DateTime.Now.ToString("dd-MM-yyyy") + ".csv";
            
            // Remove any existing double quotes from the file path
            spreadsheetFile.Path = spreadsheetFile.Path.Trim('"');
            var filePath = Path.Combine(spreadsheetFile.Path, fileName); 

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField("Registration Group");
                csv.WriteField("Support Purpose");
                csv.WriteField("External ID");
                csv.WriteField("Support Item");
                csv.WriteField("Support Item Description");
                csv.WriteField("Unit of Measure");
                csv.WriteField("Price");
                csv.WriteField("Price Control");
                csv.WriteField("Support Categories");
                csv.WriteField("Support Purpose");
                csv.NextRecord();

                foreach (var supportItem in pricelist.PricelistSupportItems)
                {
                    csv.WriteField(supportItem.RegistrationGroup);
                    csv.WriteField(supportItem.SupportPurpose);
                    csv.WriteField(supportItem.ExternalId);
                    csv.WriteField(supportItem.SupportItem);
                    csv.WriteField(supportItem.SupportItemDescription);
                    csv.WriteField(supportItem.UnitOfMeasure);
                    csv.WriteField(supportItem.Price);
                    csv.WriteField(supportItem.PriceControl);
                    csv.WriteField(supportItem.SupportCategories);
                    csv.WriteField(supportItem.SupportPurpose);
                    csv.NextRecord();
                }
            }
            return filePath;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            var error = "error";
            return error;
        }
    }
    
     public string ExportPacePricelistToCsv(Pricelist pricelist, SpreadsheetFile spreadsheetFile, RegionMenuOptions selectedRegion)
     { 
         PricelistHelper pricelistHelper = new PricelistHelper();
         try
         {
            var regionName = pricelistHelper.GetRegionDescription(selectedRegion);
            var fileName = "PACE - " + regionName + " - " + DateTime.Now.ToString("dd-MM-yyyy") + ".csv";
            
            // Remove any existing double quotes from the file path
            spreadsheetFile.Path = spreadsheetFile.Path.Trim('"');
            var filePath = Path.Combine(spreadsheetFile.Path, fileName); 

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField("Registration Group");
                csv.WriteField("Support Purpose");
                csv.WriteField("External ID");
                csv.WriteField("Support Item");
                csv.WriteField("Support Item Description");
                csv.WriteField("Unit of Measure");
                csv.WriteField("Price");
                csv.WriteField("Price Control");
                csv.WriteField("Support Categories");
                csv.WriteField("Outcome Domain");
                csv.NextRecord();

                foreach (var supportItem in pricelist.PricelistSupportItems)
                {
                    csv.WriteField(supportItem.RegistrationGroup);
                    csv.WriteField(supportItem.SupportPurpose);
                    csv.WriteField(supportItem.ExternalId);
                    csv.WriteField(supportItem.SupportItem);
                    csv.WriteField(supportItem.SupportItemDescription);
                    csv.WriteField(supportItem.UnitOfMeasure);
                    csv.WriteField(supportItem.Price);
                    csv.WriteField(supportItem.PriceControl);
                    csv.WriteField(supportItem.SupportCategories);
                    csv.WriteField(supportItem.OutcomeDomain);
                    csv.NextRecord();
                }
            }
            return filePath;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            var error = "error";
            return error;
        }
    }
}