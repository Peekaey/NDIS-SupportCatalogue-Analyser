using System.Globalization;
using PricelistGenerator.Interfaces;
using PricelistGenerator.Interfaces.Service;

namespace PricelistGenerator.Service;
using PricelistGenerator.Helpers;
using PricelistGenerator.Models;
using Spectre.Console;
public class PreviewService: IPreviewService
{
    public void ScaffoldPreviewTable(Pricelist pricelist)
    {
        var table = new Table();
        table.AddColumn("Row");
        table.AddColumn("Registration Group");
        table.AddColumn("Support Purpose");
        table.AddColumn("External ID");
        table.AddColumn("Support Item");
        table.AddColumn("Description");
        table.AddColumn("Unit of Measure");
        table.AddColumn("Price");
        table.AddColumn("Price Control");
        table.AddColumn("Support Categories");
        table.AddColumn("Support Purpose");
        
        var index = 0;
        foreach (var supportItem in pricelist.PricelistSupportItems)
        {
            var price = "";
            if (!string.IsNullOrEmpty((supportItem.Price)))
            {
                price = supportItem.Price;
            }
            else
            {
                price = "";
            }
            
            table.AddRow(
                index.ToString(),
                supportItem.RegistrationGroup,
                supportItem.SupportPurpose,
                supportItem.ExternalId,
                supportItem.SupportItem,
                "null",
                supportItem.UnitOfMeasure,
                price,
                supportItem.PriceControl,
                supportItem.SupportCategories,
                supportItem.SupportPurpose
            );
            index++;
        }
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine("Table finished rendering");
    }
    
    public void RenderPricelistAnalysisSummary(PricelistAnalysisCatalog pricelistAnalysisCatalog)
    {
    AnsiConsole.WriteLine(("Complete Pricelist Analysis"));
    AnsiConsole.WriteLine("In Summary, there has been a total of: ");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.SupportItemsAdded.Count()} Items Added");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.SupportItemsRemoved.Count()} Items Removed");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.DuplicateItemsAdded.Count()} Duplicated Items Added");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetPriceIncreaseCount()} Items with Price Increase");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetPriceDecreaseCount()} Items with Price Decrease");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetItemsRenamedCount()} Items Renamed");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetPriceControlChangeCount()} Items with Price Control Changes");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetUnitChangeCount()} Items with Unit Changes");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetProdaSupportCategoryNameChangeCount()} Items with Proda Support Category Name Change");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetPaceSupportCategoryNameChangeCount()} Items with Pace Support Category Name Change");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetProdaSupportCategoryNumberChangeCount()} Items with Proda Support Category Number Change");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetPaceSupportCategoryNumberChangeCount()} Items with Pace Support Category Number Change");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetDifferentSupportCategoryNumberCount()} Items with Different Support Category Number between Proda & Pace");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetDifferentSupportCategoryNameCount()} Items with Different Support Category Name between Proda & Pace");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetRegistrationGroupNameChangeCount()} Items with Registration Group Name Change");
    AnsiConsole.WriteLine($"- {pricelistAnalysisCatalog.GetRegistrationGroupNumberChangeCount()} Items with Registration Group Number Changes");
    }

    public void RenderPricelistAnalysisDetailed(PricelistAnalysisCatalog pricelistAnalysisCatalog)
    {

        List<Table> tables = new List<Table>();
        
        var itemsAddedTable = new Table();
        var index = 1;
        itemsAddedTable.AddColumn("Row");
        itemsAddedTable.AddColumn("Support Item Number (External Id)");
        itemsAddedTable.AddColumn("Support Item Name");
        if (pricelistAnalysisCatalog.SupportItemsAdded.Count > 0)
        {
            foreach (var item in pricelistAnalysisCatalog.SupportItemsAdded)
            {
                itemsAddedTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.SupportItemName);
                index++;
            }
        }
        tables.Add(itemsAddedTable);

        var itemsRemovedTable = new Table();
        itemsRemovedTable.AddColumn("Row");
        itemsRemovedTable.AddColumn("Support Item Number (External Id)");
        itemsRemovedTable.AddColumn("Support Item Name");
        index = 1;
        if (pricelistAnalysisCatalog.SupportItemsRemoved.Count > 0)
        {
            foreach (var item in pricelistAnalysisCatalog.SupportItemsRemoved)
            {
                itemsRemovedTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.SupportItemName);
                index++;
            }
        }
        tables.Add(itemsRemovedTable);

        var duplicateItemsAddedTable = new Table();
        duplicateItemsAddedTable.AddColumn("Row");
        duplicateItemsAddedTable.AddColumn("Support Item Number (External Id)");
        duplicateItemsAddedTable.AddColumn("Support Item Name");
        index = 1;
        if (pricelistAnalysisCatalog.DuplicateItemsAdded.Count > 0)
        {
            foreach (var item in pricelistAnalysisCatalog.DuplicateItemsAdded)
            {
                duplicateItemsAddedTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.SupportItemName);
                index++;
            }
        }
        tables.Add(duplicateItemsAddedTable);

        var priceIncreaseTable = new Table();
        priceIncreaseTable.AddColumn("Row");
        priceIncreaseTable.AddColumn("Support Item Number (External Id)");
        priceIncreaseTable.AddColumn("ACT Increase %");
        priceIncreaseTable.AddColumn("NT Increase %");
        priceIncreaseTable.AddColumn("Remote Increase %");
        priceIncreaseTable.AddColumn("Very Remote%");
        index = 1;
        if (pricelistAnalysisCatalog.SupportItemsWithPriceIncrease.Count > 0)
        {
            foreach (var item in pricelistAnalysisCatalog.SupportItemsWithPriceIncrease)
            {
                var actPercentage =
                    pricelistAnalysisCatalog.CalculatePriceIncreasePercentage(decimal.Parse(item.OldActPrice),
                        decimal.Parse(item.NewActPrice));

                var ntPercentage = pricelistAnalysisCatalog.CalculatePriceIncreasePercentage(
                    decimal.Parse(item.OldNtPrice),
                    decimal.Parse(item.NewNtPrice));

                var remotePercentage = pricelistAnalysisCatalog.CalculatePriceIncreasePercentage(
                    decimal.Parse(item.OldRemotePrice),
                    decimal.Parse(item.NewRemotePrice));

                var veryRemotePercentage = pricelistAnalysisCatalog.CalculatePriceIncreasePercentage(
                    decimal.Parse(item.OldVeryRemotePrice),
                    decimal.Parse(item.NewVeryRemotePrice));

                priceIncreaseTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                actPercentage.ToString(CultureInfo.CurrentCulture),
                ntPercentage.ToString(CultureInfo.CurrentCulture),
                remotePercentage.ToString(CultureInfo.CurrentCulture),
                veryRemotePercentage.ToString(CultureInfo.CurrentCulture));
                index++;
            }
        }
        tables.Add(priceIncreaseTable);

        var priceDecreaseTable = new Table();
        priceDecreaseTable.AddColumn("Row");
        priceDecreaseTable.AddColumn("Support Item Number (External Id)");
        priceDecreaseTable.AddColumn("ACT Decrease %");
        priceDecreaseTable.AddColumn("NT Decrease %");
        priceDecreaseTable.AddColumn("Remote Decrease %");
        priceDecreaseTable.AddColumn("Very Remote%");
        index = 1;
        if (pricelistAnalysisCatalog.SupportItemsWithPriceDecrease.Count > 0)
        {
            foreach (var item in pricelistAnalysisCatalog.SupportItemsWithPriceDecrease)
            {
                var actDecreaseString = "";
                if (item.NewActPrice != null && item.OldActPrice != null)
                {
                    var actDecrease = pricelistAnalysisCatalog.CalculatePriceDecreasePercentage(
                        decimal.Parse(item.OldActPrice),
                        decimal.Parse(item.NewActPrice));
                    actDecreaseString = actDecrease.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    actDecreaseString = "Error, Double Check Manually";
                }

                var ntDecreaseString = "";
                if (item.NewNtPrice != null && item.OldNtPrice != null)
                {
                    var ntDecrease = pricelistAnalysisCatalog.CalculatePriceDecreasePercentage(
                        decimal.Parse(item.OldNtPrice),
                        decimal.Parse(item.NewNtPrice));
                    ntDecreaseString = ntDecrease.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    ntDecreaseString = "Error, Double Check Manually";
                }

                var remoteDecreaseString = "";
                if (item.NewRemotePrice != null && item.OldRemotePrice != null)
                {
                    var remoteDecrease = pricelistAnalysisCatalog.CalculatePriceDecreasePercentage(
                        decimal.Parse(item.OldRemotePrice),
                        decimal.Parse(item.NewRemotePrice));
                    remoteDecreaseString = remoteDecrease.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    remoteDecreaseString = "Error, Double Check Manually";
                }
                
                var veryRemoteDecreaseString = "";
                if (item.NewVeryRemotePrice != null && item.OldVeryRemotePrice != null)
                {
                     var veryRemoteDecrease = pricelistAnalysisCatalog.CalculatePriceDecreasePercentage(
                        decimal.Parse(item.OldVeryRemotePrice),
                        decimal.Parse(item.NewVeryRemotePrice));
                     veryRemoteDecreaseString = veryRemoteDecrease.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    veryRemoteDecreaseString = "Error, Double Check Manually";
                }

                priceDecreaseTable.AddRow(index.ToString(),
                    item.SupportItemNumber,
                    actDecreaseString,
                    ntDecreaseString,
                    remoteDecreaseString,
                    veryRemoteDecreaseString);
                index++;
            }
        }
        tables.Add(priceDecreaseTable);

        var priceLimitChangeTable = new Table();
        priceLimitChangeTable.AddColumn("Row");
        priceLimitChangeTable.AddColumn("Support Item Number (External Id)");
        priceLimitChangeTable.AddColumn("New Price Limit");
        priceLimitChangeTable.AddColumn("Old Price Limit");
        index = 1;
        foreach (var item in pricelistAnalysisCatalog.SupportItemsWithPriceControlChanges)
        {
                priceLimitChangeTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.ActNewPriceControl,
                item.ActOldPriceControl);
            
        }
        tables.Add(priceLimitChangeTable);
        
        var itemNameChangeTable = new Table();
        itemNameChangeTable.AddColumn("Row");
        itemNameChangeTable.AddColumn("Support Item Number (External Id)");
        itemNameChangeTable.AddColumn("New Support Item Name");
        itemNameChangeTable.AddColumn("Old Support Item Name");
        index = 1;
        foreach (var item in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.NameChanged)
            {
                itemNameChangeTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.newSupportItem.SupportItemName,
                item.oldSupportItem.SupportItemName);
            }
            index++;
        }
        tables.Add(itemNameChangeTable);
        
        var unitChangeTable = new Table();
        unitChangeTable.AddColumn("Row");
        unitChangeTable.AddColumn("Support Item Number (External Id)");
        unitChangeTable.AddColumn("New Unit of Measure");
        unitChangeTable.AddColumn("Old Unit of Measure");
        index = 1;
        foreach (var item in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.UnitChanged)
            {
                unitChangeTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.newSupportItem.Unit,
                item.oldSupportItem.Unit);
            }
            index++;
        }
        tables.Add(unitChangeTable);
        
        var prodaSupportCategoryNameChangeTable = new Table();
        prodaSupportCategoryNameChangeTable.AddColumn("Row");
        prodaSupportCategoryNameChangeTable.AddColumn("Support Item Number (External Id)");
        prodaSupportCategoryNameChangeTable.AddColumn("New Proda Support Category Name");
        prodaSupportCategoryNameChangeTable.AddColumn("Old Proda Support Category Name");
        index = 1;
        foreach (var item in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.ProdaSupportCategoryNameChange)
            {
                prodaSupportCategoryNameChangeTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.newSupportItem.ProdaSupportCategoryName,
                item.oldSupportItem.ProdaSupportCategoryName);
            }
            index++;
        }
        tables.Add(prodaSupportCategoryNameChangeTable);
        
        var paceSupportCategoryNameChangeTable = new Table();
        paceSupportCategoryNameChangeTable.AddColumn("Row");
        paceSupportCategoryNameChangeTable.AddColumn("Support Item Number (External Id)");
        paceSupportCategoryNameChangeTable.AddColumn("New Pace Support Category Name");
        paceSupportCategoryNameChangeTable.AddColumn("Old Pace Support Category Name");
        index = 1;
        foreach (var item in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.PaceSupportCategoryNameChange)
            {
                paceSupportCategoryNameChangeTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.newSupportItem.PaceSupportCategoryName,
                item.oldSupportItem.PaceSupportCategoryName);
            }
        }
        tables.Add(paceSupportCategoryNameChangeTable);
        
        var prodaSupportCategoryNumberChangeTable = new Table();
        prodaSupportCategoryNumberChangeTable.AddColumn("Row");
        prodaSupportCategoryNumberChangeTable.AddColumn("Support Item Number (External Id)");
        prodaSupportCategoryNumberChangeTable.AddColumn("New Proda Support Category Number");
        prodaSupportCategoryNumberChangeTable.AddColumn("Old Proda Support Category Number");
        index = 1;
        foreach (var item in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.ProdaSupportCategoryNumberChanged)
            {
                prodaSupportCategoryNameChangeTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.newSupportItem.ProdaSupportCategoryNumber,
                item.oldSupportItem.ProdaSupportCategoryNumber);
            }
            index++;
        }
        tables.Add(prodaSupportCategoryNumberChangeTable);
        
        var paceSupportCategoryNumberChangeTable = new Table();
        paceSupportCategoryNumberChangeTable.AddColumn("Row");
        paceSupportCategoryNumberChangeTable.AddColumn("Support Item Number (External Id)");
        paceSupportCategoryNumberChangeTable.AddColumn("New Pace Support Category Number");
        paceSupportCategoryNumberChangeTable.AddColumn("Old Pace Support Category Number");
        index = 1;
        foreach (var item in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.PaceSupportCategoryNumberChanged)
            {
                paceSupportCategoryNumberChangeTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.newSupportItem.PaceSupportCategoryNumber,
                item.oldSupportItem.PaceSupportCategoryNumber);
            }
            index++;
        }
        tables.Add(paceSupportCategoryNumberChangeTable);

        var differentSupportCategoryNumberOrNameTable = new Table();
        differentSupportCategoryNumberOrNameTable.AddColumn("Row");
        differentSupportCategoryNumberOrNameTable.AddColumn("Support Item Number (External Id)");
        differentSupportCategoryNumberOrNameTable.AddColumn("Proda Support Category Number");
        differentSupportCategoryNumberOrNameTable.AddColumn("Pace Support Category Number");
        differentSupportCategoryNumberOrNameTable.AddColumn("Proda Support Category Name");
        differentSupportCategoryNumberOrNameTable.AddColumn("Pace Support Category Name");
        index = 1;
        foreach (var item in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.newSupportItem.PaceSupportCategoryNumber != item.newSupportItem.ProdaSupportCategoryNumber ||
                item.newSupportItem.PaceSupportCategoryName != item.newSupportItem.ProdaSupportCategoryName)
            {
                differentSupportCategoryNumberOrNameTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.newSupportItem.ProdaSupportCategoryNumber,
                item.newSupportItem.PaceSupportCategoryNumber,
                item.newSupportItem.ProdaSupportCategoryName,
                item.newSupportItem.PaceSupportCategoryName);
            }
            index++;
        }
        tables.Add(differentSupportCategoryNumberOrNameTable);
        
        var registrationGroupNameChangeTable = new Table();
        registrationGroupNameChangeTable.AddColumn("Row");
        registrationGroupNameChangeTable.AddColumn("Support Item Number (External Id)");
        registrationGroupNameChangeTable.AddColumn("New Registration Group Name");
        registrationGroupNameChangeTable.AddColumn("Old Registration Group Name");
        index = 1;
        foreach (var item in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.RegistrationGroupNameChange)
            {
                registrationGroupNameChangeTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.newSupportItem.RegistrationGroupName,
                item.oldSupportItem.RegistrationGroupName);
            }
            index++;
        }
        tables.Add(registrationGroupNameChangeTable);
        
        var registrationGroupNumberChangeTable = new Table();
        registrationGroupNumberChangeTable.AddColumn("Row");
        registrationGroupNumberChangeTable.AddColumn("Support Item Number (External Id)");
        registrationGroupNumberChangeTable.AddColumn("New Registration Group Number");
        registrationGroupNumberChangeTable.AddColumn("Old Registration Group Number");
        index = 1;
        foreach (var item in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.RegistrationGroupNumberChange)
            {
                registrationGroupNumberChangeTable.AddRow(index.ToString(),
                item.SupportItemNumber,
                item.newSupportItem.RegistrationGroupNumber,
                item.oldSupportItem.RegistrationGroupNumber);
            }
            index++;
        }
        tables.Add(registrationGroupNameChangeTable);
        
        List<string> tableNames = new List<string>
        {
            "Items Added",
            "Items Removed",
            "Duplicate Items Added",
            "Price Increases",
            "Price Decreases",
            "Price Limit Changes",
            "Item Name Changes",
            "Unit Changes",
            "Proda Support Category Name Changes",
            "Pace Support Category Name Changes",
            "Proda Support Category Number Changes",
            "Pace Support Category Number Changes",
            "Different Support Category Number or Name Between Pace and Proda",
            "Registration Group Name Changes",
            "Registration Group Number Changes",
        };

        for (int i = 0; i < tables.Count; i++)
        {
            AnsiConsole.WriteLine(tableNames[i]);
            AnsiConsole.Write(tables[i]);
            AnsiConsole.WriteLine("Table Finished Rendering");
            AnsiConsole.WriteLine(" ");
        }
        AnsiConsole.WriteLine("Detailed Tables Printed");
    }
    
}