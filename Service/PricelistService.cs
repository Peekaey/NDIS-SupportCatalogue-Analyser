using DocumentFormat.OpenXml.Wordprocessing;
using PricelistGenerator.Models;

namespace PricelistGenerator.Service;

public class PricelistService
{
    public Pricelist CreateProdaPricelist(NdisSupportCatalogue ndisSupportCatalogue, Pricelist pricelist,
        string selectedRegion)
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
                case "ACT":
                    pricelistSupportItem.Price = supportItem.ActPrice;
                    selectedRegionPrice = supportItem.ActPrice;
                    break;
                case "NT":
                    pricelistSupportItem.Price = supportItem.NtPrice;
                    selectedRegionPrice = supportItem.NtPrice;
                    break;
                case "Remote":
                    pricelistSupportItem.Price = supportItem.RemotePrice;
                    selectedRegionPrice = supportItem.RemotePrice;
                    break;
                case "VeryRemote":
                    pricelistSupportItem.Price = supportItem.VeryRemotePrice;
                    selectedRegionPrice = supportItem.VeryRemotePrice;
                    break;
            }

            // Getting the correct Unit Of Measure
            var unit = MapUnitOfMeasure(supportItem.Unit);
            pricelistSupportItem.UnitOfMeasure = unit;

            // Getting the correct Support Category
            pricelistSupportItem.SupportCategories = supportItem.ProdaSupportCategoryName;

            // Getting the correct Support Purpose
            var supportPurpose = MapSupportPurpose(supportItem.SupportItemNumber);
            pricelistSupportItem.SupportPurpose = supportPurpose;

            // Getting the correct Price Control
            var priceControl = MapPriceControl(selectedRegionPrice);
            pricelistSupportItem.PriceControl = priceControl;

            // Getting the correct Outcome Domain
            var outcomeDomain = MapOutcomeDomain(supportItem.SupportItemNumber);
            pricelistSupportItem.OutcomeDomain = outcomeDomain;
            
            pricelist.PricelistSupportItems.Add(pricelistSupportItem);
        }

        return pricelist;
    }
    
    public Pricelist CreatePacePricelist(NdisSupportCatalogue ndisSupportCatalogue, Pricelist pricelist,
        string selectedRegion)
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
                    case "ACT":
                        pricelistSupportItem.Price = supportItem.ActPrice;
                        selectedRegionPrice = supportItem.ActPrice;
                        break;
                    case "NT":
                        pricelistSupportItem.Price = supportItem.NtPrice;
                        selectedRegionPrice = supportItem.NtPrice;
                        break;
                    case "Remote":
                        pricelistSupportItem.Price = supportItem.RemotePrice;
                        selectedRegionPrice = supportItem.RemotePrice;
                        break;
                    case "VeryRemote":
                        pricelistSupportItem.Price = supportItem.VeryRemotePrice;
                        selectedRegionPrice = supportItem.VeryRemotePrice;
                        break;
                }

                // Getting the correct Unit Of Measure
                var unit = MapUnitOfMeasure(supportItem.Unit);
                pricelistSupportItem.UnitOfMeasure = unit;

                // Getting the correct Support Category
                pricelistSupportItem.SupportCategories = supportItem.PaceSupportCategoryName;

                // Getting the correct Support Purpose
                var supportPurpose = MapSupportPurpose(supportItem.SupportItemNumber);
                pricelistSupportItem.SupportPurpose = supportPurpose;

                // Getting the correct Price Control
                var priceControl = MapPriceControl(selectedRegionPrice);
                pricelistSupportItem.PriceControl = priceControl;

                // Getting the correct Outcome Domain
                var outcomeDomain = MapOutcomeDomain(supportItem.SupportItemNumber);
                pricelistSupportItem.OutcomeDomain = outcomeDomain;
                
                pricelist.PricelistSupportItems.Add(pricelistSupportItem);
            }
        }

        return pricelist;
    }

    public String MapOutcomeDomain(string supportItem)
    {
        string outcomeDomain = "";
        
        var thirdLastCharacter = supportItem[supportItem.Length - 3];

        if (thirdLastCharacter.ToString() == "T" || thirdLastCharacter.ToString() == "D")
        {
            //Evaluate based on the last 5 Characters instead of 3
            thirdLastCharacter = supportItem[supportItem.Length - 5];
        }
        
        switch (thirdLastCharacter.ToString())
        {
            case "1":
                outcomeDomain = "Daily Living";
                break;
            case "2":
                outcomeDomain = "Home & Placement";
                break;
            case "3":
                outcomeDomain = "Health & Wellbeing";
                break;
            case "4": 
                outcomeDomain = "Lifelong Learning & Education";
                break;
            case "5":
                outcomeDomain = "Work & Vocation";
                break;
            case "6":
                outcomeDomain = "Social & Community Participation";
                break;
            case "7":
                outcomeDomain = "Relationships, Family & Significant Others";
                break;
            case "8":
                outcomeDomain = "Choice & Control";
                break;
            default:
                outcomeDomain = "Outcome Domain Not Mapped";
                break;
            
        }
        return outcomeDomain;
    }
    
    
    public String MapPriceControl(string price)
    {
        string priceControl = "";
        
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
    
    public String MapSupportPurpose(string supportItem)
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
    

    // Redundant as Support Category is stored in the NDIS Support Catalogue
    public String MapSupportCategory(string supportItem)
    {
        // Gets the first two characters of the Support Item Number - Used as mapping for Support Category
        var supportCategoryIdentifier = supportItem.Substring(0, 2);
        switch (supportCategoryIdentifier)
        {
            case "01": 
                supportCategoryIdentifier = "Assistance with Daily Life";
                break;
            case "02":
                supportCategoryIdentifier = "Transport";
                break;
            case "03":
                supportCategoryIdentifier = "Consumables";
                break;
            case "04":
                supportCategoryIdentifier = "Assistance with social and community participation";
                break;
            case "05":
                supportCategoryIdentifier = "Assistive Technology";
                break;
            case "06":
                supportCategoryIdentifier = "Home Modification";
                break;
            case "07":
                supportCategoryIdentifier = "Support Coordination";
                break;
            case "08":
                supportCategoryIdentifier = "Improved Living Arrangements";
                break;
            case "09":
                supportCategoryIdentifier = "Increased Social and Community Participation";
                break;
            case "10":
                supportCategoryIdentifier = "Finding and Keeping a Job";
                break;
            case "11":
                supportCategoryIdentifier = "Improved Relationships";
                break;
            case "12":
                supportCategoryIdentifier = "Improved Health and Wellbeing";
                break;
            case "13":
                supportCategoryIdentifier = "Improved Learning";
                break;
            case "14":
                supportCategoryIdentifier = "Improved Life Choices";
                break;
            case "15":
                supportCategoryIdentifier = "Improved Daily Living Skills";
                break;
            default:
                supportCategoryIdentifier = "Unmapped Support Category";
                break;
        }

        return supportCategoryIdentifier;
    }

    public String MapUnitOfMeasure(string unit)
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