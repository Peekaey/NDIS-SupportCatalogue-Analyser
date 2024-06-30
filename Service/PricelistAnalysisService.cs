using System.Globalization;
using DocumentFormat.OpenXml.Math;
using PricelistGenerator.Interfaces;
using PricelistGenerator.Models;
using PricelistGenerator.Models.ExportAnalysisChanges;

namespace PricelistGenerator.Service;

public class PricelistAnalysisService : IPricelistAnalysisService
{
    public PricelistAnalysisCatalog PopulateNDISSupportCatalogue(NdisSupportCatalogue oldNdisSupportCatalog,
        NdisSupportCatalogue newNdisSupportCatalogue,
        PricelistAnalysisCatalog pricelistAnalysisCatalogue)
    {
        pricelistAnalysisCatalogue = MapNDISSupportCatalogueItems(oldNdisSupportCatalog, newNdisSupportCatalogue
            , pricelistAnalysisCatalogue);
        
        pricelistAnalysisCatalogue = GetSupportItemsAdded(pricelistAnalysisCatalogue, newNdisSupportCatalogue);
        pricelistAnalysisCatalogue = GetSupportItemsRemoved(pricelistAnalysisCatalogue, newNdisSupportCatalogue);
        pricelistAnalysisCatalogue = GetDuplicateItemsAdded(pricelistAnalysisCatalogue, newNdisSupportCatalogue);
        pricelistAnalysisCatalogue = GetSupportItemNameChange(pricelistAnalysisCatalogue, newNdisSupportCatalogue);
        pricelistAnalysisCatalogue =
            GetSupportItemProdaSupportCategoryNameChange(pricelistAnalysisCatalogue, newNdisSupportCatalogue);
        pricelistAnalysisCatalogue =
            GetSupportItemProdaSupportCategoryNumberChange(pricelistAnalysisCatalogue, newNdisSupportCatalogue);
        pricelistAnalysisCatalogue =
            GetSupportItemPaceSupportCategoryNameChange(pricelistAnalysisCatalogue, newNdisSupportCatalogue);
        pricelistAnalysisCatalogue =
            GetSupportItemPaceSupportCategoryNumberChange(pricelistAnalysisCatalogue, newNdisSupportCatalogue);
        pricelistAnalysisCatalogue = GetPriceChanges(pricelistAnalysisCatalogue);
        pricelistAnalysisCatalogue = GetUnitChanges(pricelistAnalysisCatalogue);
        pricelistAnalysisCatalogue = GetPriceControlChanges(pricelistAnalysisCatalogue);
        pricelistAnalysisCatalogue = GetRegistrationGroupNameChange(pricelistAnalysisCatalogue);
        pricelistAnalysisCatalogue = GetRegistrationGroupNumberChange(pricelistAnalysisCatalogue);
        pricelistAnalysisCatalogue = GetSupportPurposeChange(pricelistAnalysisCatalogue);
        pricelistAnalysisCatalogue = CalculatePriceIncreasePercentage(pricelistAnalysisCatalogue);
        pricelistAnalysisCatalogue = CalculatePriceDecreasePercentage(pricelistAnalysisCatalogue);
        
        return pricelistAnalysisCatalogue;
    }

    private PricelistAnalysisCatalog MapNDISSupportCatalogueItems(NdisSupportCatalogue oldNdisSupportCatalog,
        NdisSupportCatalogue newNdisSupportCatalog, PricelistAnalysisCatalog pricelistAnalysisCatalogue)
    {
        pricelistAnalysisCatalogue.pricelistAnalysisCatalogSupportItems =
            new List<PricelistAnalysisCatalogSupportItem>();
        // Mapping old supportCatalogue first

        foreach (var supportItem in oldNdisSupportCatalog.NdisSupportCatalogueSupportItems)
        {
            var pricelistAnalysisSupportItem = new PricelistAnalysisCatalogSupportItem();

            pricelistAnalysisSupportItem.SupportItemNumber = supportItem.SupportItemNumber;
            pricelistAnalysisSupportItem.oldSupportItem = supportItem;
            pricelistAnalysisSupportItem.newSupportItem = supportItem;
            pricelistAnalysisCatalogue.pricelistAnalysisCatalogSupportItems.Add(pricelistAnalysisSupportItem);
        }

        return pricelistAnalysisCatalogue;
    }

    private PricelistAnalysisCatalog GetSupportItemsAdded(PricelistAnalysisCatalog pricelistAnalysisCatalog,
        NdisSupportCatalogue newNdisSupportCatalogue)
    {
        pricelistAnalysisCatalog.SupportItemsAdded = new List<NdisSupportCatalogueSupportItem>();
        foreach (var newSupportItem in newNdisSupportCatalogue.NdisSupportCatalogueSupportItems)
        {
            var found = false;
            foreach (var oldSupportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
                if (oldSupportItem.SupportItemNumber == newSupportItem.SupportItemNumber)
                {
                    // Means item exists in old and new pricelist - break and make copy from new pricelist
                    oldSupportItem.newSupportItem = newSupportItem;
                    found = true;
                    break;
                }

            if (!found)
                // Item exists in new pricelist but not old - new item
                pricelistAnalysisCatalog.SupportItemsAdded.Add(newSupportItem);
        }

        return pricelistAnalysisCatalog;
    }

    private PricelistAnalysisCatalog GetSupportItemsRemoved(PricelistAnalysisCatalog pricelistAnalysisCatalog,
        NdisSupportCatalogue newNdisSupportCatalogue)
    {
        pricelistAnalysisCatalog.SupportItemsRemoved = new List<NdisSupportCatalogueSupportItem>();
        foreach (var oldSupportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            var found = false;
            foreach (var newSupportItem in newNdisSupportCatalogue.NdisSupportCatalogueSupportItems)
                if (oldSupportItem.SupportItemNumber == newSupportItem.SupportItemNumber)
                {
                    // Means old item exists in new old and new pricelist - break and no need to take 
                    // new copy as it is already taken in GetSupportItemsAdded
                    found = true;
                    break;
                }

            if (!found)
                // Means item exists in old pricelist but not in new pricelist - removed item
                pricelistAnalysisCatalog.SupportItemsRemoved.Add(oldSupportItem.oldSupportItem);
        }

        return pricelistAnalysisCatalog;
    }

    private PricelistAnalysisCatalog GetDuplicateItemsAdded(PricelistAnalysisCatalog pricelistAnalysisCatalog,
        NdisSupportCatalogue newNdisSupportCatalogue)
    {
        pricelistAnalysisCatalog.DuplicateItemsAdded = new List<NdisSupportCatalogueSupportItem>();
     
        var seenItems = new Dictionary<string, NdisSupportCatalogueSupportItem>();

        foreach (var newSupportItem in newNdisSupportCatalogue.NdisSupportCatalogueSupportItems)
        {
            if (seenItems.ContainsKey(newSupportItem.SupportItemNumber))
            {
                pricelistAnalysisCatalog.DuplicateItemsAdded.Add(newSupportItem);
            }
            else
            {
                seenItems.Add(newSupportItem.SupportItemNumber, newSupportItem);
            }
        }

        return pricelistAnalysisCatalog;
    }

    private PricelistAnalysisCatalog GetSupportItemNameChange(PricelistAnalysisCatalog pricelistAnalysisCatalog,
        NdisSupportCatalogue newNdisSupportCatalogue)
    {
        foreach (var pricelistAnalysisSupportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        foreach (var newSupportItem in newNdisSupportCatalogue.NdisSupportCatalogueSupportItems)
            if (newSupportItem.SupportItemNumber == pricelistAnalysisSupportItem.SupportItemNumber)
                if (!newSupportItem.SupportItemName.Equals(pricelistAnalysisSupportItem.oldSupportItem.SupportItemName))
                    pricelistAnalysisSupportItem.NameChanged = true;
        return pricelistAnalysisCatalog;
    }

    private PricelistAnalysisCatalog GetSupportItemProdaSupportCategoryNameChange(
        PricelistAnalysisCatalog pricelistAnalysisCatalog
        , NdisSupportCatalogue newNdisSupportCatalogue)
    {
        foreach (var supportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
            if (supportItem.oldSupportItem.ProdaSupportCategoryName !=
                supportItem.newSupportItem.ProdaSupportCategoryName)
                supportItem.ProdaSupportCategoryNameChange = true;
        return pricelistAnalysisCatalog;
    }

    private PricelistAnalysisCatalog GetSupportItemPaceSupportCategoryNameChange(
        PricelistAnalysisCatalog pricelistAnalysisCatalog
        , NdisSupportCatalogue newNdisSupportCatalogue)
    {
        foreach (var supportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
            if (supportItem.oldSupportItem.PaceSupportCategoryName !=
                supportItem.newSupportItem.PaceSupportCategoryName)
                supportItem.PaceSupportCategoryNameChange = true;
        return pricelistAnalysisCatalog;
    }

    private PricelistAnalysisCatalog GetSupportItemProdaSupportCategoryNumberChange(
        PricelistAnalysisCatalog pricelistAnalysisCatalog
        , NdisSupportCatalogue newNdisSupportCatalogue)
    {
        foreach (var supportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
            if (supportItem.oldSupportItem.ProdaSupportCategoryNumber !=
                supportItem.newSupportItem.ProdaSupportCategoryNumber)
                supportItem.ProdaSupportCategoryNumberChanged = true;
        return pricelistAnalysisCatalog;
    }

    private PricelistAnalysisCatalog GetSupportItemPaceSupportCategoryNumberChange(
        PricelistAnalysisCatalog pricelistAnalysisCatalog
        , NdisSupportCatalogue newNdisSupportCatalogue)
    {
        foreach (var supportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
            if (supportItem.oldSupportItem.PaceSupportCategoryNumber !=
                supportItem.newSupportItem.PaceSupportCategoryNumber)
                supportItem.PaceSupportCategoryNumberChanged = true;
        return pricelistAnalysisCatalog;
    }

    private PricelistAnalysisCatalog GetPriceChanges(PricelistAnalysisCatalog pricelistAnalysisCatalog)
    {
        pricelistAnalysisCatalog.SupportItemsWithPriceIncrease =
            new List<PricelistAnalysisSupportItemsWithPriceChanges>();

        pricelistAnalysisCatalog.SupportItemsWithPriceDecrease =
            new List<PricelistAnalysisSupportItemsWithPriceChanges>();
        
        foreach (var supportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            var pricelistAnalysisSupportItemsWithPriceChanges
                = new PricelistAnalysisSupportItemsWithPriceChanges();

            var priceChange = false;
            var priceIncrease = false;

            // Prices across regions only increase or decrease as a group, never individually
            pricelistAnalysisSupportItemsWithPriceChanges.SupportItemNumber = supportItem.SupportItemNumber;
            if (!string.IsNullOrEmpty(supportItem.oldSupportItem.ActPrice)&& !string.IsNullOrEmpty(supportItem.newSupportItem.ActPrice))
            {
                if (supportItem.oldSupportItem.ActPrice != supportItem.newSupportItem.ActPrice)
                {
                    if (decimal.Parse(supportItem.newSupportItem.ActPrice) >
                        decimal.Parse(supportItem.oldSupportItem.ActPrice))
                    {
                        priceIncrease = true;
                        supportItem.ActPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Increased;
                    }
                    else
                    {
                        supportItem.ActPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Decreased;
                    }
                    pricelistAnalysisSupportItemsWithPriceChanges.NewActPrice = supportItem.newSupportItem.ActPrice;
                    pricelistAnalysisSupportItemsWithPriceChanges.OldActPrice = supportItem.oldSupportItem.ActPrice;
                    priceChange = true;
                }
                else
                {
                    supportItem.ActPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Unchanged;
                }
            }

            if (!string.IsNullOrEmpty(supportItem.oldSupportItem.NswPrice) &&
                !string.IsNullOrEmpty(supportItem.newSupportItem.NswPrice))
            {
                if (supportItem.oldSupportItem.NswPrice != supportItem.newSupportItem.NswPrice)
                {
                    if (decimal.Parse(supportItem.newSupportItem.NswPrice) >
                        decimal.Parse(supportItem.oldSupportItem.NswPrice))
                    {
                        priceIncrease = true;
                        supportItem.NswPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Increased;
                    }
                    else
                    {
                        supportItem.NswPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Decreased;
                    }
                    pricelistAnalysisSupportItemsWithPriceChanges.NewNswPrice = supportItem.newSupportItem.NswPrice;
                    pricelistAnalysisSupportItemsWithPriceChanges.OldNswPrice = supportItem.oldSupportItem.NswPrice;
                    priceChange = true;
                }
                else
                {
                    supportItem.NswPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Unchanged;
                }
            }

            if (!string.IsNullOrEmpty(supportItem.oldSupportItem.VicPrice) &&
                !string.IsNullOrEmpty(supportItem.newSupportItem.VicPrice))
            {
                if (supportItem.oldSupportItem.VicPrice != supportItem.newSupportItem.VicPrice)
                {
                    if (decimal.Parse(supportItem.newSupportItem.VicPrice) >
                        decimal.Parse(supportItem.oldSupportItem.VicPrice))
                    {
                        priceIncrease = true;
                        supportItem.VicPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Increased;
                    }
                    else
                    {
                        supportItem.VicPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Decreased;
                    }
                    pricelistAnalysisSupportItemsWithPriceChanges.NewVicPrice = supportItem.newSupportItem.VicPrice;
                    pricelistAnalysisSupportItemsWithPriceChanges.OldVicPrice = supportItem.oldSupportItem.VicPrice;
                    priceChange = true;
                }
                else
                {
                    supportItem.VicPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Unchanged;
                }
            }
            
            if(!string.IsNullOrEmpty(supportItem.oldSupportItem.QldPrice) && !string.IsNullOrEmpty(supportItem.newSupportItem.QldPrice)) {
                if (supportItem.oldSupportItem.QldPrice != supportItem.newSupportItem.QldPrice)
                {
                    if (decimal.Parse(supportItem.newSupportItem.QldPrice) >
                        decimal.Parse(supportItem.oldSupportItem.QldPrice))
                    {
                        priceIncrease = true;
                        supportItem.QldPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Increased;
                    }
                    else
                    {
                        supportItem.QldPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Decreased;
                    }
                    pricelistAnalysisSupportItemsWithPriceChanges.NewQldPrice = supportItem.newSupportItem.QldPrice;
                    pricelistAnalysisSupportItemsWithPriceChanges.OldQldPrice = supportItem.oldSupportItem.QldPrice;
                    priceChange = true;
                }
                else
                {
                    supportItem.QldPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Unchanged;
                }
            }

            if (!string.IsNullOrEmpty(supportItem.oldSupportItem.SaPrice) &&
                !string.IsNullOrEmpty(supportItem.newSupportItem.SaPrice))
            {
                if (supportItem.oldSupportItem.SaPrice != supportItem.newSupportItem.SaPrice)
                {
                    if (decimal.Parse(supportItem.newSupportItem.SaPrice) >
                        decimal.Parse(supportItem.oldSupportItem.SaPrice))
                    {
                        priceIncrease = true;
                        supportItem.SaPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Increased;
                    }
                    else
                    {
                        supportItem.SaPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Decreased;
                    }
                    pricelistAnalysisSupportItemsWithPriceChanges.NewSaPrice = supportItem.newSupportItem.SaPrice;
                    pricelistAnalysisSupportItemsWithPriceChanges.OldSaPrice = supportItem.oldSupportItem.SaPrice;
                    priceChange = true;
                }
                else
                {
                    supportItem.SaPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Unchanged;
                }
            }

            if (!string.IsNullOrEmpty(supportItem.oldSupportItem.TasPrice) &&
                !string.IsNullOrEmpty(supportItem.newSupportItem.TasPrice)) {
                if (supportItem.oldSupportItem.TasPrice != supportItem.newSupportItem.TasPrice)
                {
                    if (decimal.Parse(supportItem.newSupportItem.TasPrice) >
                        decimal.Parse(supportItem.oldSupportItem.TasPrice))
                    {
                        priceIncrease = true;
                        supportItem.TasPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Increased;
                    }
                    else
                    {
                        supportItem.TasPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Decreased;
                    }
                    pricelistAnalysisSupportItemsWithPriceChanges.NewTasPrice = supportItem.newSupportItem.TasPrice;
                    pricelistAnalysisSupportItemsWithPriceChanges.OldTasPrice = supportItem.oldSupportItem.TasPrice;
                    priceChange = true;
                }
                else
                {
                    supportItem.TasPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Unchanged;
                }
            }

            if (!string.IsNullOrEmpty(supportItem.oldSupportItem.WaPrice) &&
                !string.IsNullOrEmpty(supportItem.newSupportItem.WaPrice))
            {
                if (supportItem.oldSupportItem.WaPrice != supportItem.newSupportItem.WaPrice)
                {
                    if (decimal.Parse(supportItem.newSupportItem.WaPrice) >
                        decimal.Parse(supportItem.oldSupportItem.WaPrice))
                    {
                        priceIncrease = true;
                        supportItem.WaPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Increased;
                    }
                    else
                    {
                        supportItem.WaPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Decreased;
                    }
                    pricelistAnalysisSupportItemsWithPriceChanges.NewWaPrice = supportItem.newSupportItem.WaPrice;
                    pricelistAnalysisSupportItemsWithPriceChanges.OldWaPrice = supportItem.oldSupportItem.WaPrice;
                    priceChange = true;
                }
                else
                {
                    supportItem.WaPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Unchanged;
                }
            }

            if (!string.IsNullOrEmpty(supportItem.oldSupportItem.NtPrice) && !string.IsNullOrEmpty(supportItem.newSupportItem.NtPrice)) {
                if (supportItem.oldSupportItem.NtPrice != supportItem.newSupportItem.NtPrice)
                {
                    if (decimal.Parse(supportItem.newSupportItem.NtPrice) >
                        decimal.Parse(supportItem.oldSupportItem.NtPrice))
                    {
                        priceIncrease = true;
                        supportItem.NtPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Increased;
                    }
                    else
                    {
                        supportItem.NtPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Decreased;
                    }
                    pricelistAnalysisSupportItemsWithPriceChanges.NewNtPrice = supportItem.newSupportItem.NtPrice;
                    pricelistAnalysisSupportItemsWithPriceChanges.OldNtPrice = supportItem.oldSupportItem.NtPrice;
                    priceChange = true;
                }
                else
                {
                    supportItem.NtPriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Unchanged;
                }
            }

            if (!string.IsNullOrEmpty(supportItem.oldSupportItem.RemotePrice) &&
                !string.IsNullOrEmpty(supportItem.newSupportItem.RemotePrice))
            {
                if (supportItem.oldSupportItem.RemotePrice != supportItem.newSupportItem.RemotePrice)
                {
                    if (decimal.Parse(supportItem.newSupportItem.RemotePrice) >
                        decimal.Parse(supportItem.oldSupportItem.RemotePrice))
                    {
                        priceIncrease = true;
                        supportItem.RemotePriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Increased;
                    }
                    else
                    {
                        supportItem.RemotePriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Decreased;
                    }
                    pricelistAnalysisSupportItemsWithPriceChanges.NewRemotePrice = supportItem.newSupportItem.RemotePrice;
                    pricelistAnalysisSupportItemsWithPriceChanges.OldRemotePrice = supportItem.oldSupportItem.RemotePrice;
                    priceChange = true;
                }
                else
                {
                    supportItem.RemotePriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Unchanged;
                }
            }
            
            if (!string.IsNullOrEmpty(supportItem.oldSupportItem.VeryRemotePrice) &&
                !string.IsNullOrEmpty(supportItem.newSupportItem.VeryRemotePrice))
            {
                if (supportItem.oldSupportItem.VeryRemotePrice != supportItem.newSupportItem.VeryRemotePrice)
                {
                    if (decimal.Parse(supportItem.newSupportItem.VeryRemotePrice) >
                        decimal.Parse(supportItem.oldSupportItem.VeryRemotePrice))
                    {
                        priceIncrease = true;
                        supportItem.VeryRemotePriceChange =
                            PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Increased;
                    }
                    else
                    {
                        supportItem.VeryRemotePriceChange =
                            PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Decreased;
                    }
                    pricelistAnalysisSupportItemsWithPriceChanges.NewVeryRemotePrice =
                        supportItem.newSupportItem.VeryRemotePrice;
                    pricelistAnalysisSupportItemsWithPriceChanges.OldVeryRemotePrice =
                        supportItem.oldSupportItem.VeryRemotePrice;
                    priceChange = true;
                }
                else
                {
                    supportItem.VeryRemotePriceChange = PricelistAnalysisCatalogSupportItem.PriceChangeStatus.Unchanged;
                }
            }
            
            if (priceChange)
            {
                if (priceIncrease)
                {
                    pricelistAnalysisCatalog.SupportItemsWithPriceIncrease.Add(pricelistAnalysisSupportItemsWithPriceChanges);
                }
                else
                {
                    pricelistAnalysisCatalog.SupportItemsWithPriceDecrease.Add(pricelistAnalysisSupportItemsWithPriceChanges);
                }
            }
        }
        return pricelistAnalysisCatalog;
    }

    private PricelistAnalysisCatalog GetUnitChanges(PricelistAnalysisCatalog pricelistAnalysisCatalog)
    {
        foreach (var supportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
            if (supportItem.oldSupportItem.Unit != supportItem.newSupportItem.Unit)
                supportItem.UnitChanged = true;
        return pricelistAnalysisCatalog;
    }

    private PricelistAnalysisCatalog GetPriceControlChanges(PricelistAnalysisCatalog pricelistAnalysisCatalog)
    {
        pricelistAnalysisCatalog.SupportItemsWithPriceControlChanges =
            new List<PricelistAnalysisPriceControlChanges>();
        
        foreach (var supportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            var priceChange = false;
            var oldActPriceControl = GetPriceControl(supportItem.oldSupportItem.ActPrice);
            var oldNswPriceControl = GetPriceControl(supportItem.oldSupportItem.NswPrice);
            var oldVicPriceControl = GetPriceControl(supportItem.oldSupportItem.VicPrice);
            var oldQldPriceControl = GetPriceControl(supportItem.oldSupportItem.QldPrice);
            var oldSaPriceControl = GetPriceControl(supportItem.oldSupportItem.SaPrice);
            var oldTasPriceControl = GetPriceControl(supportItem.oldSupportItem.TasPrice);
            var oldWaPriceControl = GetPriceControl(supportItem.oldSupportItem.WaPrice);
            var oldNtPriceControl = GetPriceControl(supportItem.oldSupportItem.NtPrice);
            var oldRemotePriceControl = GetPriceControl(supportItem.oldSupportItem.RemotePrice);
            var oldVeryRemotePriceControl = GetPriceControl(supportItem.oldSupportItem.VeryRemotePrice);

            var newActPriceControl = GetPriceControl(supportItem.newSupportItem.ActPrice);
            var newNswPriceControl = GetPriceControl(supportItem.newSupportItem.NswPrice);
            var newVicPriceControl = GetPriceControl(supportItem.newSupportItem.VicPrice);
            var newQldPriceControl = GetPriceControl(supportItem.newSupportItem.QldPrice);
            var newSaPriceControl = GetPriceControl(supportItem.newSupportItem.SaPrice);
            var newTasPriceControl = GetPriceControl(supportItem.newSupportItem.TasPrice);
            var newWaPriceControl = GetPriceControl(supportItem.newSupportItem.WaPrice);
            var newNtPriceControl = GetPriceControl(supportItem.newSupportItem.NtPrice);
            var newRemotePriceControl = GetPriceControl(supportItem.newSupportItem.RemotePrice);
            var newVeryRemotePriceControl = GetPriceControl(supportItem.newSupportItem.VeryRemotePrice);


            if (oldActPriceControl != newActPriceControl)
            {
                priceChange = true;
            }

            if (oldNswPriceControl != newNswPriceControl)
            {
                priceChange = true;
            }

            if (oldVicPriceControl != newVicPriceControl)
            {
                priceChange = true;
            }

            if (oldQldPriceControl != newQldPriceControl)
            {
                priceChange = true;
            }

            if (oldSaPriceControl != newSaPriceControl)
            {
                priceChange = true;
            }

            if (oldTasPriceControl != newTasPriceControl)
            {
                priceChange = true;
            }

            if (oldWaPriceControl != newWaPriceControl)
            {
                priceChange = true;
            }

            if (oldNtPriceControl != newNtPriceControl)
            {
                priceChange = true;
            }

            if (oldRemotePriceControl != newRemotePriceControl)
            {
                priceChange = true;
            }

            if (oldVeryRemotePriceControl != newVeryRemotePriceControl)
            {
                priceChange = true;
            }

            if (priceChange)
            {
                PricelistAnalysisPriceControlChanges pricelistAnalysisPriceControlChanges =
                    new PricelistAnalysisPriceControlChanges();
                pricelistAnalysisPriceControlChanges.SupportItemNumber = supportItem.SupportItemNumber;
                pricelistAnalysisPriceControlChanges.ActOldPriceControl = oldActPriceControl;
                pricelistAnalysisPriceControlChanges.ActNewPriceControl = newActPriceControl;
                pricelistAnalysisPriceControlChanges.NswOldPriceControl = oldNswPriceControl;
                pricelistAnalysisPriceControlChanges.NswNewPriceControl = newNswPriceControl;
                pricelistAnalysisPriceControlChanges.VicOldPriceControl = oldVicPriceControl;
                pricelistAnalysisPriceControlChanges.VicNewPriceControl = newVicPriceControl;
                pricelistAnalysisPriceControlChanges.QldOldPriceControl = oldQldPriceControl;
                pricelistAnalysisPriceControlChanges.QldNewPriceControl = newQldPriceControl;
                pricelistAnalysisPriceControlChanges.SaOldPriceControl = oldSaPriceControl;
                pricelistAnalysisPriceControlChanges.SaNewPriceControl = newSaPriceControl;
                pricelistAnalysisPriceControlChanges.TasOldPriceControl = oldTasPriceControl;
                pricelistAnalysisPriceControlChanges.TasNewPriceControl = newTasPriceControl;
                pricelistAnalysisPriceControlChanges.WaOldPriceControl = oldWaPriceControl;
                pricelistAnalysisPriceControlChanges.WaNewPriceControl = newWaPriceControl;
                pricelistAnalysisPriceControlChanges.NtOldPriceControl = oldNtPriceControl;
                pricelistAnalysisPriceControlChanges.NtNewPriceControl = newNtPriceControl;
                pricelistAnalysisPriceControlChanges.RemoteOldPriceControl = oldRemotePriceControl;
                pricelistAnalysisPriceControlChanges.RemoteNewPriceControl = newRemotePriceControl;
                pricelistAnalysisPriceControlChanges.VeryRemoteOldPriceControl = oldVeryRemotePriceControl;
                pricelistAnalysisPriceControlChanges.VeryRemoteNewPriceControl = newVeryRemotePriceControl;
                pricelistAnalysisCatalog.SupportItemsWithPriceControlChanges.Add(pricelistAnalysisPriceControlChanges);
            }
        }
        return pricelistAnalysisCatalog;
    }
    
    private PricelistAnalysisCatalog GetRegistrationGroupNameChange(PricelistAnalysisCatalog pricelistAnalysisCatalog)
    {
        foreach (var supportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
            if (supportItem.oldSupportItem.RegistrationGroupName != supportItem.newSupportItem.RegistrationGroupName)
                supportItem.RegistrationGroupNameChange = true;
        return pricelistAnalysisCatalog;
    }

    private PricelistAnalysisCatalog GetRegistrationGroupNumberChange(PricelistAnalysisCatalog pricelistAnalysisCatalog)
    {
        foreach (var supportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
            if (supportItem.oldSupportItem.RegistrationGroupNumber !=
                supportItem.newSupportItem.RegistrationGroupNumber)
                supportItem.RegistrationGroupNumberChange = true;
        return pricelistAnalysisCatalog;
    }

    private PricelistAnalysisCatalog GetSupportPurposeChange(PricelistAnalysisCatalog pricelistAnalysisCatalog)
    {
        foreach (var supportItem in pricelistAnalysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            var oldSupportPurpose = GetSupportPurpose(supportItem.oldSupportItem.SupportItemNumber);
            var newSupportPurpose = GetSupportPurpose(supportItem.newSupportItem.SupportItemNumber);
            if (oldSupportPurpose != newSupportPurpose)
            {
                supportItem.SupportPurposeChanged = true;
            }
        }
        return pricelistAnalysisCatalog;
    }


    private string GetPriceControl(string? price)
    {
        if (string.IsNullOrEmpty(price) || price == "1.00")
        {
            return "Recommended";
        }
        else
        {
            return "Maximum";
        }
    }

    private string GetSupportPurpose(string supportItem)
    {
        var identifier = supportItem.Substring(supportItem.Length - 1);
        // Uses the generic identifier to identify the support Purpose if the support item does not end with _T
        if (!supportItem.Contains("T"))
            switch (identifier)
            {
                case "1":
                    return "Core";
                case "2":
                    return "Capital";
                case "3":
                    return "Capacity Building";
                default:
                    return "Unmapped Support Purpose";
            }

        // Mapping for support items that end with _T or _D > Changes to third last character
        identifier = supportItem.Substring(supportItem.Length - 3);
        switch (identifier)
        {
            case "1_T":
                return "Core";
                break;
            case "1_D":
                return "Core";
                break;
            // Represents Identifier 1_T_D
            case "T_D":
                return "Core";
                break;
            default:
                return "Unmapped Support Purpose";
                break;
        }
    }

    private PricelistAnalysisCatalog CalculatePriceIncreasePercentage(PricelistAnalysisCatalog pricelistAnalysisCatalog)
    {
        foreach (var item in pricelistAnalysisCatalog.SupportItemsWithPriceIncrease)
        {
            var actPercentage =
                pricelistAnalysisCatalog.CalculatePriceIncreasePercentage(decimal.Parse(item.OldActPrice),
                    decimal.Parse(item.NewActPrice));
            item.ActPercentage = actPercentage.ToString(CultureInfo.CurrentCulture);

            var ntPercentage = pricelistAnalysisCatalog.CalculatePriceIncreasePercentage(
                decimal.Parse(item.OldNtPrice),
                decimal.Parse(item.NewNtPrice));
            item.NtPercentage = ntPercentage.ToString(CultureInfo.CurrentCulture);

            var remotePercentage = pricelistAnalysisCatalog.CalculatePriceIncreasePercentage(
                decimal.Parse(item.OldRemotePrice),
                decimal.Parse(item.NewRemotePrice));
            item.RemotePercentage = remotePercentage.ToString(CultureInfo.CurrentCulture);
            
            var veryRemotePercentage = pricelistAnalysisCatalog.CalculatePriceIncreasePercentage(
                decimal.Parse(item.OldVeryRemotePrice),
                decimal.Parse(item.NewVeryRemotePrice));
            item.VeryRemotePercentage = veryRemotePercentage.ToString(CultureInfo.CurrentCulture);
        }

        return pricelistAnalysisCatalog;
    }
    
    private PricelistAnalysisCatalog CalculatePriceDecreasePercentage(PricelistAnalysisCatalog pricelistAnalysisCatalog)
    {
        foreach (var item in pricelistAnalysisCatalog.SupportItemsWithPriceDecrease)
        {
            if (item.NewActPrice != null && item.OldActPrice != null)
            {
                var actPercentage =
                    pricelistAnalysisCatalog.CalculatePriceDecreasePercentage(decimal.Parse(item.OldActPrice),
                        decimal.Parse(item.NewActPrice));
                item.ActPercentage = actPercentage.ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                item.ActPercentage = "Error, Double Check Manually";
            }

            if (item.NewNtPrice != null && item.OldNtPrice != null)
            {
                var ntPercentage = pricelistAnalysisCatalog.CalculatePriceDecreasePercentage(
                    decimal.Parse(item.OldNtPrice),
                    decimal.Parse(item.NewNtPrice));
                item.NtPercentage = ntPercentage.ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                item.NtPercentage = "Error, Double Check Manually";
            }

            if (item.NewRemotePrice != null && item.OldRemotePrice != null)
            {
                var remotePercentage = pricelistAnalysisCatalog.CalculatePriceDecreasePercentage(
                    decimal.Parse(item.OldRemotePrice),
                    decimal.Parse(item.NewRemotePrice));
                item.RemotePercentage = remotePercentage.ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                item.RemotePercentage = "Error, Double Check Manually";
            }

            if (item.NewVeryRemotePrice != null && item.OldVeryRemotePrice != null)
            {
                var veryRemotePercentage = pricelistAnalysisCatalog.CalculatePriceDecreasePercentage(
                    decimal.Parse(item.OldVeryRemotePrice),
                    decimal.Parse(item.NewVeryRemotePrice));
                item.VeryRemotePercentage = veryRemotePercentage.ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                item.VeryRemotePercentage = "Error, Double Check Manually";
            }
        }

        return pricelistAnalysisCatalog;
    }

    public ExportAnalysisChanges MapPricelistAnalysisCatalogToExportAnalysis(PricelistAnalysisCatalog analysisCatalog)
    {
        ExportAnalysisChanges exportAnalysisChanges = new ExportAnalysisChanges();
        
        exportAnalysisChanges.ItemsAdded = new List<BaseItem>();
        foreach (var itemAdded in analysisCatalog.SupportItemsAdded)
        {
            BaseItem baseItem = new BaseItem();
            baseItem.SupportItemNumber = itemAdded.SupportItemNumber;
            baseItem.NewSupportItemName = itemAdded.SupportItemName;
            exportAnalysisChanges.ItemsAdded.Add(baseItem);
        }

        exportAnalysisChanges.ItemsRemoved = new List<BaseItem>();
        foreach (var itemRemoved in analysisCatalog.SupportItemsRemoved)
        {
            BaseItem baseItem = new BaseItem();
            baseItem.SupportItemNumber = itemRemoved.SupportItemNumber;
            baseItem.NewSupportItemName = itemRemoved.SupportItemName;
            exportAnalysisChanges.ItemsRemoved.Add(baseItem);
        }
        
        exportAnalysisChanges.DuplicateItems= new List<BaseItem>();
        foreach (var duplicateItem in analysisCatalog.DuplicateItemsAdded)
        {
            BaseItem baseItem = new BaseItem();
            baseItem.SupportItemNumber = duplicateItem.SupportItemNumber;
            baseItem.NewSupportItemName = duplicateItem.SupportItemName;
            exportAnalysisChanges.DuplicateItems.Add(baseItem);
        }
        
        exportAnalysisChanges.PriceIncreases = new List<PriceChange>();
        foreach (var item in analysisCatalog.SupportItemsWithPriceIncrease)
        {
            PriceChange priceChange = new PriceChange();
            priceChange.SupportItemNumber = item.SupportItemNumber;
            priceChange.ActPriceChangePercentage = item.ActPercentage;
            priceChange.NtPriceChangePercentage = item.NtPercentage;
            priceChange.RemotePriceChangePercentage = item.RemotePercentage;
            priceChange.VeryRemotePriceChangePercentage = item.VeryRemotePercentage;
            exportAnalysisChanges.PriceIncreases.Add(priceChange);
        }
        
        exportAnalysisChanges.PriceDecreases = new List<PriceChange>();
        foreach (var item in analysisCatalog.SupportItemsWithPriceDecrease)
        {
            PriceChange priceChange = new PriceChange();
            priceChange.SupportItemNumber = item.SupportItemNumber;
            priceChange.ActPriceChangePercentage = item.ActPercentage;
            priceChange.NtPriceChangePercentage = item.NtPercentage;
            priceChange.RemotePriceChangePercentage = item.RemotePercentage;
            priceChange.VeryRemotePriceChangePercentage = item.VeryRemotePercentage;
            exportAnalysisChanges.PriceDecreases.Add(priceChange);
        }
        
        exportAnalysisChanges.NameChanges = new List<ItemNameChange>();
        foreach(var item in analysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.NameChanged)
            {
                ItemNameChange itemNameChange = new ItemNameChange();
                itemNameChange.SupportItemNumber = item.SupportItemNumber;
                itemNameChange.NewSupportItemName = item.newSupportItem.SupportItemName;
                itemNameChange.OldSupportItemName = item.oldSupportItem.SupportItemName;
                exportAnalysisChanges.NameChanges.Add(itemNameChange);
            }
        }
        
        exportAnalysisChanges.PriceLimitChanges = new List<PriceLimitChange>();
        foreach (var item in analysisCatalog.SupportItemsWithPriceControlChanges)
        {
            PriceLimitChange priceLimitChange = new PriceLimitChange();
            priceLimitChange.SupportItemNumber = item.SupportItemNumber;
            priceLimitChange.NewPriceLimit = item.ActNewPriceControl;
            priceLimitChange.OldPriceLimit = item.ActOldPriceControl;
            exportAnalysisChanges.PriceLimitChanges.Add(priceLimitChange);
        }

        exportAnalysisChanges.UnitChanges = new List<UnitOfMeasureChange>();
        foreach (var item in analysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.UnitChanged)
            {
                UnitOfMeasureChange unitOfMeasureChange = new UnitOfMeasureChange();
                unitOfMeasureChange.SupportItemNumber = item.SupportItemNumber;
                unitOfMeasureChange.NewUnitOfMeasure = item.newSupportItem.Unit;
                unitOfMeasureChange.OldUnitOfMeasure = item.oldSupportItem.Unit;
                exportAnalysisChanges.UnitChanges.Add(unitOfMeasureChange);
            }
        }
        
        exportAnalysisChanges.PaceSupportCategoryNameChanges = new List<SupportCategoryNameChange>();
        foreach (var item in analysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.PaceSupportCategoryNameChange)
            {
                SupportCategoryNameChange supportCategoryNameChange = new SupportCategoryNameChange();
                supportCategoryNameChange.SupportItemNumber = item.SupportItemNumber;
                supportCategoryNameChange.NewSupportCategoryName = item.newSupportItem.PaceSupportCategoryName;
                supportCategoryNameChange.OldSupportCategoryName = item.oldSupportItem.PaceSupportCategoryName;
                exportAnalysisChanges.PaceSupportCategoryNameChanges.Add(supportCategoryNameChange);
            }
        }
        
        exportAnalysisChanges.ProdaSupportCategoryNameChanges = new List<SupportCategoryNameChange>();
        foreach (var item in analysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.ProdaSupportCategoryNameChange)
            {
                SupportCategoryNameChange supportCategoryNameChange = new SupportCategoryNameChange();
                supportCategoryNameChange.SupportItemNumber = item.SupportItemNumber;
                supportCategoryNameChange.NewSupportCategoryName = item.newSupportItem.ProdaSupportCategoryName;
                supportCategoryNameChange.OldSupportCategoryName = item.oldSupportItem.ProdaSupportCategoryName;
                exportAnalysisChanges.ProdaSupportCategoryNameChanges.Add(supportCategoryNameChange);
            }
        }
        
        exportAnalysisChanges.PaceSupportCategoryNumberChanges = new List<SupportCategoryNumberChange>();
        foreach (var item in analysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.PaceSupportCategoryNumberChanged)
            {
                SupportCategoryNumberChange supportCategoryNumberChange = new SupportCategoryNumberChange();
                supportCategoryNumberChange.SupportItemNumber = item.SupportItemNumber;
                supportCategoryNumberChange.NewSupportCategoryNumber = item.newSupportItem.PaceSupportCategoryNumber;
                supportCategoryNumberChange.OldSupportCategoryNumber = item.oldSupportItem.PaceSupportCategoryNumber;
                exportAnalysisChanges.PaceSupportCategoryNumberChanges.Add(supportCategoryNumberChange);
            }
        }
        
        exportAnalysisChanges.ProdaSupportCategoryNumberChanges = new List<SupportCategoryNumberChange>();
        foreach (var item in analysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.ProdaSupportCategoryNumberChanged)
            {
                SupportCategoryNumberChange supportCategoryNumberChange = new SupportCategoryNumberChange();
                supportCategoryNumberChange.SupportItemNumber = item.SupportItemNumber;
                supportCategoryNumberChange.NewSupportCategoryNumber = item.newSupportItem.ProdaSupportCategoryNumber;
                supportCategoryNumberChange.OldSupportCategoryNumber = item.oldSupportItem.ProdaSupportCategoryNumber;
                exportAnalysisChanges.ProdaSupportCategoryNumberChanges.Add(supportCategoryNumberChange);
            }
        }
        
        exportAnalysisChanges.RegistrationGroupNameChanges = new List<RegistrationGroupNameChange>();
        foreach (var item in analysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.RegistrationGroupNameChange)
            {
                RegistrationGroupNameChange registrationGroupNameChange = new RegistrationGroupNameChange();
                registrationGroupNameChange.SupportItemNumber = item.SupportItemNumber;
                registrationGroupNameChange.NewRegistrationGroupName = item.newSupportItem.RegistrationGroupName;
                registrationGroupNameChange.OldRegistrationGroupName = item.oldSupportItem.RegistrationGroupName;
                exportAnalysisChanges.RegistrationGroupNameChanges.Add(registrationGroupNameChange);
            }
        }
        
        exportAnalysisChanges.RegistrationGroupNumberChanges = new List<RegistrationGroupNumberChange>();
        foreach (var item in analysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.RegistrationGroupNumberChange)
            {
                RegistrationGroupNumberChange registrationGroupNumberChange = new RegistrationGroupNumberChange();
                registrationGroupNumberChange.SupportItemNumber = item.SupportItemNumber;
                registrationGroupNumberChange.NewRegistrationGroupNumber = item.newSupportItem.RegistrationGroupNumber;
                registrationGroupNumberChange.OldRegistrationGroupNumber = item.oldSupportItem.RegistrationGroupNumber;
                exportAnalysisChanges.RegistrationGroupNumberChanges.Add(registrationGroupNumberChange);
            }
        }

        exportAnalysisChanges.DifferentSupportCategoryNumberOrNames = new List<DifferentSupportCategoryNumberOrName>();
        foreach (var item in analysisCatalog.pricelistAnalysisCatalogSupportItems)
        {
            if (item.newSupportItem.ProdaSupportCategoryName != item.newSupportItem.PaceSupportCategoryName ||
                item.newSupportItem.ProdaSupportCategoryNumber != item.newSupportItem.PaceSupportCategoryNumber)
            {
                DifferentSupportCategoryNumberOrName differentSupportCategoryNumberOrName =
                    new DifferentSupportCategoryNumberOrName();
                differentSupportCategoryNumberOrName.SupportItemNumber = item.SupportItemNumber;
                differentSupportCategoryNumberOrName.ProdaSupportCategoryName = item.newSupportItem.ProdaSupportCategoryName;
                differentSupportCategoryNumberOrName.ProdaSupportCategoryNumber = item.newSupportItem.ProdaSupportCategoryNumber;
                differentSupportCategoryNumberOrName.PaceSupportCategoryName = item.newSupportItem.PaceSupportCategoryName;
                differentSupportCategoryNumberOrName.PaceSupportCategoryNumber = item.newSupportItem.PaceSupportCategoryNumber;
                exportAnalysisChanges.DifferentSupportCategoryNumberOrNames.Add(differentSupportCategoryNumberOrName);
            }
        }
        return exportAnalysisChanges;
    }
}