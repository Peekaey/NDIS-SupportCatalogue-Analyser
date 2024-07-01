using ClosedXML.Excel;
using PricelistGenerator.Interfaces.Helpers;
using PricelistGenerator.Models.ExportAnalysisChanges;
using PricelistGenerator.Models.File;
using Spectre.Console;

namespace PricelistGenerator.Helpers;

public class ExcelHelper: IExcelHelper
{
    
    public bool ValidateProvidedFile(string providedFilePath)
    {
        string trimmedFilePath = RemoveQuotesFromFilePath(providedFilePath);

        if (!CheckFileExists(trimmedFilePath))
        {
            return false;
        }

        return CheckFileExtensionValid(trimmedFilePath);
    }
    
    private bool CheckFileExists(string providedFilePath)
    {
        return File.Exists(providedFilePath);
    }
    
    private bool CheckFileExtensionValid(string providedFilePath)
    {
        string fileExtension = GetFileExtension(providedFilePath);
        
        bool isValidExtension = !string.IsNullOrEmpty(fileExtension) &&
                                string.Equals(fileExtension, ".xlsx", StringComparison.OrdinalIgnoreCase);
        
        return isValidExtension;
    }
    
    private String GetFileExtension(string providedFilePath)
    {
        try
        {
            return Path.GetExtension(providedFilePath);
        }
        catch (Exception e)
        {
            throw new Exception("Error getting file extension", e);
        }
    }
    
    public SpreadsheetFile CreateFileFromProvidedFilePath(string providedFilePath)
    {
        string trimmedFilePath = RemoveQuotesFromFilePath(providedFilePath);
        SpreadsheetFile spreadsheetFile = new SpreadsheetFile();

        try
        {
            spreadsheetFile.Name = Path.GetFileName(providedFilePath);
            spreadsheetFile.Extension = GetFileExtension(providedFilePath);
            spreadsheetFile.FullFilePath = trimmedFilePath;
            spreadsheetFile.Path = Path.GetDirectoryName(providedFilePath);
            
        }
        catch (Exception e)
        {
            throw new Exception("Error creating file from provided file path: " + e.Message);
        }

        return spreadsheetFile;
    }
    
    private String RemoveQuotesFromFilePath(string providedFilePath)
    {
        if (providedFilePath.StartsWith("\"") && providedFilePath.EndsWith("\""))
        {
            providedFilePath = providedFilePath.Trim('"');
        }
        return providedFilePath;
    }

    public void ExportAnalysisToExcel(ExportAnalysisChanges exportAnalysisChanges, SpreadsheetFile spreadsheetFile)
    {
        var fileName = "PricelistAnalysis Changes" + " - " + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx";
            
        // Remove any existing double quotes from the file path
        spreadsheetFile.Path = spreadsheetFile.Path.Trim('"');
        var filePath = Path.Combine(spreadsheetFile.Path, fileName); 
        
        try {
            using (XLWorkbook workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet1 = workbook.Worksheets.Add("Items Added");
                worksheet1.Cell(1, 1).Value = "Support Item Number";
                worksheet1.Cell(1, 2).Value = "Support Item Name";
                if (exportAnalysisChanges.ItemsAdded.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.ItemsAdded.Count; i++)
                    {
                        worksheet1.Cell(i + 2, 1).Value = exportAnalysisChanges.ItemsAdded[i].SupportItemNumber;
                        worksheet1.Cell(i + 2, 2).Value = exportAnalysisChanges.ItemsAdded[i].NewSupportItemName;
                    }
                }

                if (exportAnalysisChanges.ItemsRemoved.Count > 0)
                {
                    IXLWorksheet worksheet2 = workbook.Worksheets.Add("Items Removed");
                    worksheet2.Cell(1, 1).Value = "Support Item Number";
                    worksheet2.Cell(1, 2).Value = "Support Item Name";
                    for (var i = 0; i < exportAnalysisChanges.ItemsRemoved.Count; i++)
                    {
                        worksheet2.Cell(i + 2, 1).Value = exportAnalysisChanges.ItemsRemoved[i].SupportItemNumber;
                        worksheet2.Cell(i + 2, 2).Value = exportAnalysisChanges.ItemsRemoved[i].NewSupportItemName;
                    }
                }

                if (exportAnalysisChanges.DuplicateItems.Count > 0)
                {
                    IXLWorksheet worksheet3 = workbook.Worksheets.Add("Duplicate Items");
                    worksheet3.Cell(1, 1).Value = "Support Item Number";
                    worksheet3.Cell(1, 2).Value = "Support Item Name";
                    for (var i = 0; i < exportAnalysisChanges.DuplicateItems.Count; i++)
                    {
                        worksheet3.Cell(i + 2, 1).Value = exportAnalysisChanges.DuplicateItems[i].SupportItemNumber;
                        worksheet3.Cell(i + 2, 2).Value = exportAnalysisChanges.DuplicateItems[i].NewSupportItemName;
                    }
                }

                IXLWorksheet worksheet4 = workbook.Worksheets.Add("Price Increase");
                worksheet4.Cell(1, 1).Value = "Support Item Number";
                worksheet4.Cell(1, 2).Value = "Act Price Increase Percentage";
                worksheet4.Cell(1, 3).Value = "Nt Price Increase Percentage";
                worksheet4.Cell(1, 4).Value = "Remote Price Increase Percentage";
                worksheet4.Cell(1, 5).Value = "Very Remote Price Increase Percentage";
                if (exportAnalysisChanges.PriceIncreases.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.PriceIncreases.Count; i++)
                    {
                        worksheet4.Cell(i + 2, 1).Value = exportAnalysisChanges.PriceIncreases[i].SupportItemNumber;
                        worksheet4.Cell(i + 2, 2).Value =
                            exportAnalysisChanges.PriceIncreases[i].ActPriceChangePercentage;
                        worksheet4.Cell(i + 2, 3).Value =
                            exportAnalysisChanges.PriceIncreases[i].NtPriceChangePercentage;
                        worksheet4.Cell(i + 2, 4).Value =
                            exportAnalysisChanges.PriceIncreases[i].RemotePriceChangePercentage;
                        worksheet4.Cell(i + 2, 5).Value =
                            exportAnalysisChanges.PriceIncreases[i].VeryRemotePriceChangePercentage;
                    }
                }

                IXLWorksheet worksheet5 = workbook.Worksheets.Add("Price Decrease");
                worksheet5.Cell(1, 1).Value = "Support Item Number";
                worksheet5.Cell(1, 2).Value = "Act Price Decrease Percentage";
                worksheet5.Cell(1, 3).Value = "Nt Price Decrease Percentage";
                worksheet5.Cell(1, 4).Value = "Remote Price Decrease Percentage";
                worksheet5.Cell(1, 5).Value = "Very Remote Price Decrease Percentage";
                if (exportAnalysisChanges.PriceDecreases.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.PriceDecreases.Count; i++)
                    {
                        worksheet5.Cell(i + 2, 1).Value = exportAnalysisChanges.PriceDecreases[i].SupportItemNumber;
                        worksheet5.Cell(i + 2, 2).Value =
                            exportAnalysisChanges.PriceDecreases[i].ActPriceChangePercentage;
                        worksheet5.Cell(i + 2, 3).Value =
                            exportAnalysisChanges.PriceDecreases[i].NtPriceChangePercentage;
                        worksheet5.Cell(i + 2, 4).Value =
                            exportAnalysisChanges.PriceDecreases[i].RemotePriceChangePercentage;
                        worksheet5.Cell(i + 2, 5).Value =
                            exportAnalysisChanges.PriceDecreases[i].VeryRemotePriceChangePercentage;
                    }
                }

                IXLWorksheet worksheet6 = workbook.Worksheets.Add("Name Changes");
                worksheet6.Cell(1, 1).Value = "Support Item Number";
                worksheet6.Cell(1, 2).Value = "New Support Item Name";
                worksheet6.Cell(1, 3).Value = "Old Support Item Name";
                if (exportAnalysisChanges.NameChanges.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.NameChanges.Count; i++)
                    {
                        worksheet6.Cell(i + 2, 1).Value = exportAnalysisChanges.NameChanges[i].SupportItemNumber;
                        worksheet6.Cell(i + 2, 2).Value = exportAnalysisChanges.NameChanges[i].NewSupportItemName;
                        worksheet6.Cell(i + 2, 3).Value = exportAnalysisChanges.NameChanges[i].OldSupportItemName;
                    }
                }

                IXLWorksheet worksheet7 = workbook.Worksheets.Add("Price Limit Changes");
                worksheet7.Cell(1, 1).Value = "Support Item Number";
                worksheet7.Cell(1, 2).Value = "New Price Limit";
                worksheet7.Cell(1, 3).Value = "Old Price Limit";
                if (exportAnalysisChanges.PriceLimitChanges.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.PriceLimitChanges.Count; i++)
                    {
                        worksheet7.Cell(i + 2, 1).Value = exportAnalysisChanges.PriceLimitChanges[i].SupportItemNumber;
                        worksheet7.Cell(i + 2, 2).Value = exportAnalysisChanges.PriceLimitChanges[i].NewPriceLimit;
                        worksheet7.Cell(i + 2, 3).Value = exportAnalysisChanges.PriceLimitChanges[i].OldPriceLimit;
                    }
                } 
                
                IXLWorksheet worksheet8 = workbook.Worksheets.Add("Unit Changes");
                worksheet8.Cell(1, 1).Value = "Support Item Number";
                worksheet8.Cell(1, 2).Value = "New Unit";
                worksheet8.Cell(1, 3).Value = "Old Unit";
                if (exportAnalysisChanges.UnitChanges.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.UnitChanges.Count; i++)
                    {
                        worksheet8.Cell(i + 2, 1).Value = exportAnalysisChanges.UnitChanges[i].SupportItemNumber;
                        worksheet8.Cell(i + 2, 2).Value = exportAnalysisChanges.UnitChanges[i].NewUnitOfMeasure;
                        worksheet8.Cell(i + 2, 3).Value = exportAnalysisChanges.UnitChanges[i].OldUnitOfMeasure;
                    }
                }
                
                IXLWorksheet worksheet9 = workbook.Worksheets.Add("Pace Category Number Changes");
                worksheet9.Cell(1, 1).Value = "Support Item Number";
                worksheet9.Cell(1, 2).Value = "New Pace Support Category Number";
                worksheet9.Cell(1, 3).Value = "Old Pace Support Category Number";
                if (exportAnalysisChanges.PaceSupportCategoryNumberChanges.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.PaceSupportCategoryNumberChanges.Count; i++)
                    {
                        worksheet9.Cell(i + 2, 1).Value =
                            exportAnalysisChanges.PaceSupportCategoryNumberChanges[i].SupportItemNumber;
                        worksheet9.Cell(i + 2, 2).Value = exportAnalysisChanges.PaceSupportCategoryNumberChanges[i]
                            .NewSupportCategoryNumber;
                        worksheet9.Cell(i + 2, 3).Value = exportAnalysisChanges.PaceSupportCategoryNumberChanges[i]
                            .OldSupportCategoryNumber;
                    }
                }

                IXLWorksheet worksheet10 = workbook.Worksheets.Add("Proda Category Number Changes");
                worksheet10.Cell(1, 1).Value = "Support Item Number";
                worksheet10.Cell(1, 2).Value = "New Proda Support Category Number";
                worksheet10.Cell(1, 3).Value = "Old Proda Support Category Number";
                if (exportAnalysisChanges.ProdaSupportCategoryNumberChanges.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.ProdaSupportCategoryNumberChanges.Count; i++)
                    {
                        worksheet10.Cell(i + 2, 1).Value =
                            exportAnalysisChanges.ProdaSupportCategoryNumberChanges[i].SupportItemNumber;
                        worksheet10.Cell(i + 2, 2).Value = exportAnalysisChanges.ProdaSupportCategoryNumberChanges[i]
                            .NewSupportCategoryNumber;
                        worksheet10.Cell(i + 2, 3).Value = exportAnalysisChanges.ProdaSupportCategoryNumberChanges[i]
                            .OldSupportCategoryNumber;
                    }
                }

                IXLWorksheet worksheet11 = workbook.Worksheets.Add("Pace Category Name Changes");
                worksheet11.Cell(1, 1).Value = "Support Item Number";
                worksheet11.Cell(1, 2).Value = "New Pace Support Category Name";
                worksheet11.Cell(1, 3).Value = "Old Pace Support Category Name";
                if (exportAnalysisChanges.PaceSupportCategoryNameChanges.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.PaceSupportCategoryNameChanges.Count; i++)
                    {
                        worksheet11.Cell(i + 2, 1).Value =
                            exportAnalysisChanges.PaceSupportCategoryNameChanges[i].SupportItemNumber;
                        worksheet11.Cell(i + 2, 2).Value =
                            exportAnalysisChanges.PaceSupportCategoryNameChanges[i].NewSupportCategoryName;
                        worksheet11.Cell(i + 2, 3).Value =
                            exportAnalysisChanges.PaceSupportCategoryNameChanges[i].OldSupportCategoryName;
                    }
                }

                IXLWorksheet worksheet12 = workbook.Worksheets.Add("Proda Category Name Changes");
                worksheet12.Cell(1, 1).Value = "Support Item Number";
                worksheet12.Cell(1, 2).Value = "New Proda Support Category Name";
                worksheet12.Cell(1, 3).Value = "Old Proda Support Category Name";
                if (exportAnalysisChanges.ProdaSupportCategoryNameChanges.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.ProdaSupportCategoryNameChanges.Count; i++)
                    {
                        worksheet12.Cell(i + 2, 1).Value =
                            exportAnalysisChanges.ProdaSupportCategoryNameChanges[i].SupportItemNumber;
                        worksheet12.Cell(i + 2, 2).Value = exportAnalysisChanges.ProdaSupportCategoryNameChanges[i]
                            .NewSupportCategoryName;
                        worksheet12.Cell(i + 2, 3).Value = exportAnalysisChanges.ProdaSupportCategoryNameChanges[i]
                            .OldSupportCategoryName;
                    }
                }

                IXLWorksheet worksheet13 = workbook.Worksheets.Add("Diff Cat Num and Nam Changes");
                worksheet13.Cell(1, 1).Value = "Support Item Number";
                worksheet13.Cell(1, 2).Value = "Pace Support Category Number";
                worksheet13.Cell(1, 3).Value = "Proda Support Category Number";
                worksheet13.Cell(1, 4).Value = "Pace Support Category Name";
                worksheet13.Cell(1, 5).Value = "Proda Support Category Name";
                if (exportAnalysisChanges.DifferentSupportCategoryNumberOrNames.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.DifferentSupportCategoryNumberOrNames.Count; i++)
                    {
                        worksheet13.Cell(i + 2, 1).Value = exportAnalysisChanges
                            .DifferentSupportCategoryNumberOrNames[i]
                            .SupportItemNumber;
                        worksheet13.Cell(i + 2, 3).Value = exportAnalysisChanges
                            .DifferentSupportCategoryNumberOrNames[i]
                            .PaceSupportCategoryNumber;
                        worksheet13.Cell(i + 2, 2).Value = exportAnalysisChanges
                            .DifferentSupportCategoryNumberOrNames[i]
                            .ProdaSupportCategoryNumber;
                        worksheet13.Cell(i + 2, 4).Value = exportAnalysisChanges
                            .DifferentSupportCategoryNumberOrNames[i]
                            .PaceSupportCategoryName;
                        worksheet13.Cell(i + 2, 5).Value = exportAnalysisChanges
                            .DifferentSupportCategoryNumberOrNames[i]
                            .ProdaSupportCategoryName;
                    }
                }

                IXLWorksheet worksheet14 = workbook.Worksheets.Add("Registration Name Changes");
                worksheet14.Cell(1, 1).Value = "Support Item Number";
                worksheet14.Cell(1, 3).Value = "New Registration Group Name";
                worksheet14.Cell(1, 2).Value = "Old Registration Group Name";
                if (exportAnalysisChanges.RegistrationGroupNameChanges.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.RegistrationGroupNameChanges.Count; i++)
                    {
                        worksheet14.Cell(i + 2, 1).Value =
                            exportAnalysisChanges.RegistrationGroupNameChanges[i].SupportItemNumber;
                        worksheet14.Cell(i + 2, 3).Value =
                            exportAnalysisChanges.RegistrationGroupNameChanges[i].NewRegistrationGroupName;
                        worksheet14.Cell(i + 2, 2).Value =
                            exportAnalysisChanges.RegistrationGroupNameChanges[i].OldRegistrationGroupName;
                    }
                }

                IXLWorksheet worksheet15 = workbook.Worksheets.Add("Registration Number Changes");
                worksheet15.Cell(1, 1).Value = "Support Item Number";
                worksheet15.Cell(1, 3).Value = "New Registration Group Number";
                worksheet15.Cell(1, 2).Value = "Old Registration Group Number";
                if (exportAnalysisChanges.RegistrationGroupNumberChanges.Count > 0)
                {
                    for (var i = 0; i < exportAnalysisChanges.RegistrationGroupNumberChanges.Count; i++)
                    {
                        worksheet15.Cell(i + 2, 1).Value =
                            exportAnalysisChanges.RegistrationGroupNumberChanges[i].SupportItemNumber;
                        worksheet15.Cell(i + 2, 3).Value = exportAnalysisChanges.RegistrationGroupNumberChanges[i]
                            .NewRegistrationGroupNumber;
                        worksheet15.Cell(i + 2, 2).Value = exportAnalysisChanges.RegistrationGroupNumberChanges[i]
                            .OldRegistrationGroupNumber;
                    }
                }
                workbook.SaveAs(filePath);
                AnsiConsole.WriteLine("File has been exported to: " + filePath);
            } 
        } catch (Exception e)
        {
            AnsiConsole.WriteLine("Error exporting analysis to Excel: " + e.Message);
        }
    }
}