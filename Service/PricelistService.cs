using DocumentFormat.OpenXml.Wordprocessing;
using PricelistGenerator.Interfaces;
using PricelistGenerator.Interfaces.Service;
using PricelistGenerator.Models;
using PricelistGenerator.Models.Menu;

namespace PricelistGenerator.Service;

public class PricelistService: IPricelistService
{
    public Pricelist CreateProdaPricelist(NdisSupportCatalogue ndisSupportCatalogue, Pricelist pricelist,
        RegionMenuOptions selectedRegion)
    {
        foreach (var supportItem in ndisSupportCatalogue.NdisSupportCatalogueSupportItems)
        {
            PricelistSupportItem pricelistSupportItem = new PricelistSupportItem();

            pricelistSupportItem.ExternalId = supportItem.SupportItemNumber;
            pricelistSupportItem.SupportItem = supportItem.SupportItemName;
            pricelistSupportItem.RegistrationGroup = supportItem.RegistrationGroupName;
            
            var selectedRegionPrice = "";
            switch (selectedRegion)
            {
                case RegionMenuOptions.Act:
                    pricelistSupportItem.Price = supportItem.ActPrice;
                    selectedRegionPrice = supportItem.ActPrice;
                    break;
                case RegionMenuOptions.Nt:
                    pricelistSupportItem.Price = supportItem.NtPrice;
                    selectedRegionPrice = supportItem.NtPrice;
                    break;
                case RegionMenuOptions.Remote:
                    pricelistSupportItem.Price = supportItem.RemotePrice;
                    selectedRegionPrice = supportItem.RemotePrice;
                    break;
                case RegionMenuOptions.VeryRemote:
                    pricelistSupportItem.Price = supportItem.VeryRemotePrice;
                    selectedRegionPrice = supportItem.VeryRemotePrice;
                    break;
            }
            
            var unit = MapUnitOfMeasure(supportItem.Unit);
            pricelistSupportItem.UnitOfMeasure = unit;
            
            pricelistSupportItem.SupportCategories = supportItem.ProdaSupportCategoryName;
            
            var supportPurpose = MapSupportPurpose(supportItem.SupportItemNumber);
            pricelistSupportItem.SupportPurpose = supportPurpose;
            
            var priceControl = MapPriceControl(selectedRegionPrice);
            pricelistSupportItem.PriceControl = priceControl;
            
            var outcomeDomain = MapOutcomeDomain(supportItem.SupportItemNumber);
            pricelistSupportItem.OutcomeDomain = outcomeDomain;
            
            pricelist.PricelistSupportItems.Add(pricelistSupportItem);
        }
        return pricelist;
    }
    
    public Pricelist CreatePacePricelist(NdisSupportCatalogue ndisSupportCatalogue, Pricelist pricelist,
        RegionMenuOptions selectedRegion)
    {
        foreach (var supportItem in ndisSupportCatalogue.NdisSupportCatalogueSupportItems)
        {
            if (supportItem.PaceSupportCategoryNumber != supportItem.ProdaSupportCategoryNumber 
                || supportItem.PaceSupportCategoryName != supportItem.ProdaSupportCategoryName)
            { 
                PricelistSupportItem pricelistSupportItem = new PricelistSupportItem();

                pricelistSupportItem.ExternalId = supportItem.SupportItemNumber + "_PACE";
                pricelistSupportItem.SupportItem = supportItem.SupportItemName;
                pricelistSupportItem.RegistrationGroup = supportItem.RegistrationGroupName;
                
                var selectedRegionPrice = "";
                switch (selectedRegion)
                {
                    case RegionMenuOptions.Act :
                        pricelistSupportItem.Price = supportItem.ActPrice;
                        selectedRegionPrice = supportItem.ActPrice;
                        break;
                    case RegionMenuOptions.Nt:
                        pricelistSupportItem.Price = supportItem.NtPrice;
                        selectedRegionPrice = supportItem.NtPrice;
                        break;
                    case RegionMenuOptions.Remote:
                        pricelistSupportItem.Price = supportItem.RemotePrice;
                        selectedRegionPrice = supportItem.RemotePrice;
                        break;
                    case RegionMenuOptions.VeryRemote:
                        pricelistSupportItem.Price = supportItem.VeryRemotePrice;
                        selectedRegionPrice = supportItem.VeryRemotePrice;
                        break;
                }
                
                var unit = MapUnitOfMeasure(supportItem.Unit);
                pricelistSupportItem.UnitOfMeasure = unit;
                
                pricelistSupportItem.SupportCategories = supportItem.PaceSupportCategoryName;
                
                var supportPurpose = MapSupportPurpose(supportItem.SupportItemNumber);
                pricelistSupportItem.SupportPurpose = supportPurpose;
                
                var priceControl = MapPriceControl(selectedRegionPrice);
                pricelistSupportItem.PriceControl = priceControl;
                
                var outcomeDomain = MapOutcomeDomain(supportItem.SupportItemNumber);
                pricelistSupportItem.OutcomeDomain = outcomeDomain;
                
                pricelist.PricelistSupportItems.Add(pricelistSupportItem);
            }
        }

        return pricelist;
    }

    private String MapOutcomeDomain(string supportItem)
    {
        var outcomeDomain = "";
        
        // supportItem = supportItem.Replace("_", "");
        var thirdLastCharacter = supportItem[supportItem.Length - 3];

        if (thirdLastCharacter.ToString() == "T" || thirdLastCharacter.ToString() == "D")
        {
            //Evaluate based on the last 5 Characters instead of 3
            thirdLastCharacter = supportItem[supportItem.Length - 5];
        }
        
        int number;
        if (int.TryParse(thirdLastCharacter.ToString(), out number))
        {
            switch (number)
            {
                case 1:
                    outcomeDomain = "Daily Living";
                    break;
                case 2:
                    outcomeDomain = "Home & Placement";
                    break;
                case 3:
                    outcomeDomain = "Health & Wellbeing";
                    break;
                case 4:
                    outcomeDomain = "Lifelong Learning & Education";
                    break;
                case 5:
                    outcomeDomain = "Work & Vocation";
                    break;
                case 6:
                    outcomeDomain = "Social & Community Participation";
                    break;
                case 7:
                    outcomeDomain = "Relationships, Family & Significant Others";
                    break;
                case 8:
                    outcomeDomain = "Choice & Control";
                    break;
                default:
                    outcomeDomain = "Outcome Domain Not Mapped";
                    break;
            }
        }
        return outcomeDomain;
    }
    
    private String MapPriceControl(string price)
    {
        var priceControl = "";
        
        switch (price)
        {
            case "1":
                priceControl = "Recommended";
                break;
            case null:
                priceControl = "Recommended";
                break;
            default:
                priceControl = "Maximum";
                break;
        }
        return priceControl;
    }
    
    private String MapSupportPurpose(string supportItem)
    {
        string supportPurpose = "";
        string supportPurposeIdentifier = supportItem.Substring(supportItem.Length - 1);
        // Uses the generic identifier to identify the support Purpose if the support item does not end with _T
        if (!supportItem.Contains("T"))
        {
            switch (supportPurposeIdentifier)
            {
                case "1":
                    supportPurpose = "Core";
                    break;
                case "2":
                    supportPurpose = "Capital";
                    break;
                case "3":
                    supportPurpose = "Capacity Building";
                    break;
                default: 
                    supportPurpose = "Unmapped Support Purpose";
                    break;
            }
            return supportPurpose;
        }
        else
        { 
            // Mapping for support items that end with _T or _D > Changes to third last character
           supportPurposeIdentifier = supportItem.Substring(supportItem.Length - 3);
           switch (supportPurposeIdentifier)
           {
                case "1_T":
                    supportPurpose = "Core";
                    break;
                case "1_D":
                    supportPurpose = "Core";
                    break;
                // Represents Identifier 1_T_D
                case"T_D":
                    supportPurpose = "Core";
                    break;
                default: 
                    supportPurpose = "Unmapped Support Purpose";
                    break;
           }
           return supportPurpose;
        }
    }
    
    private String MapUnitOfMeasure(string unit)
    {
        switch (unit)
        {
            case "E":
                unit = "Each";
                break;
            case "H":
                unit = "Hour";
                break;
            case "D":
                unit = "Day";
                break;
            case "WK":
                unit = "Week";
                break;
            case "MON":
                unit = "Monthly";
                break;
            case "YR":
                unit = "Annual";
                break;
            default:
                unit = "Unmapped Unit of Measure";
                break;
        }
        return unit;
    }
}