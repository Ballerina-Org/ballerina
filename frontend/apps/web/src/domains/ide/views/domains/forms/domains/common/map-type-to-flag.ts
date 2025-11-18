import {Map} from "immutable";
import {IdeFlags, TypeMapping} from "./ide-flags.ts";

export function mapTypeToFlags({
                                   typeStr,
                                   ancestors,
                               }: TypeMapping): IdeFlags | undefined {
    if (!typeStr && !ancestors) {
        // TODO: find a better way to warn about this without cluttering the console
        // console.warn("Called mapTypeToFlags with no typeStr or ancestors");
        return undefined;
    }

    const ancestorToFlagsMap: Record<string, IdeFlags> = {
        OneTimeBusinessPartnerDetails: {
            // must be above BusinessPartner in the list, so that it matches first
            // and only locks and refreshes the one-time business partner details (CPD)
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [], //DashboardCardType.SenderCard],
        },
        BusinessPartner: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [], //DashboardCardType.AccountingCard],
        },
        InvoiceAccountingPosition: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [],
        },
        InvoicePosition: {
            kind: "localAndRemote",
            customLocks: ["OpenPositions"],
            lockedCards: [
                //DashboardCardType.PositionsCard,
                //DashboardCardType.ExtraCostsCard,
            ],
        },
        OrderConfirmationExtraCostsPositions: {
            kind: "localAndRemote",
            customLocks: ["OpenPositions"],
            lockedCards: [], //DashboardCardType.ExtraCostsCard],
        },
        InvoiceExtraCostsPositions: {
            kind: "localAndRemote",
            customLocks: ["OpenPositions"],
            lockedCards: [], //DashboardCardType.ExtraCostsCard],
        },
        SalesOrderPosition: {
            kind: "localAndRemote",
            customLocks: ["OpenPositions"],
            lockedCards: [],
        },
        OrderConfirmationPosition: {
            kind: "localAndRemote",
            customLocks: ["OpenPositions"],
            lockedCards: [
                //DashboardCardType.PositionsCard,
                //DashboardCardType.ExtraCostsCard,
            ],
        },
        DeliveryNotePosition: {
            kind: "localAndRemote",
            customLocks: ["OpenPositions"],
            lockedCards: [],
        },
        PaymentSectionMultiTax: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [],
        },
    };

    for (const ancestor of Object.keys(ancestorToFlagsMap)) {
        if (ancestors?.includes(ancestor)) return ancestorToFlagsMap[ancestor];
    }

    if (!typeStr) {
        // TODO: find a better way to warn about this without cluttering the console
        // console.warn(
        //   "Called mapTypeToFlags with ancestors only but no map item matched"
        // );
        return undefined;
    }

    const typeToFlagsMap = Map<string, IdeFlags>({
        TenantRef: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [
                //DashboardCardType.SenderCard,
                // DashboardCardType.BankDetailsCard,
                // DashboardCardType.PaymentCard,
                // DashboardCardType.PositionsCard,
                // DashboardCardType.AccountingCard,
            ],
        },
        BusinessPartnerWithMetadataAndDetails: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [
                //DashboardCardType.BankDetailsCard,
                //DashboardCardType.PaymentCard,
                //DashboardCardType.PositionsCard,
                //DashboardCardType.AccountingCard,
            ],
        },
        DocumentTypeRef: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: {}
            //     Object.values(DashboardCardType).filter(
            //     (card) => card != DashboardCardType.InformationCard
            // ),
        },
        InvoiceTypeRef: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: {}
            //     Object.values(DashboardCardType).filter(
            //     (card) => card != DashboardCardType.InformationCard
            // ),
        },
        PaymentSectionTotals: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [],
        },
        InvoiceAccounting: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [], //DashboardCardType.AccountingCard],
        },
        HistoricalInvoiceAccounting: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [], //DashboardCardType.AccountingCard],
        },
        OpenInvoicePosition: {
            kind: "localAndRemote",
            customLocks: ["OpenPositions"],
            lockedCards: [
                //DashboardCardType.PositionsCard,
                //DashboardCardType.ExtraCostsCard,
            ],
        },
        OpenPurchaseOrderPosition: {
            kind: "localAndRemote",
            customLocks: ["OpenPositions"],
            lockedCards: [
                //DashboardCardType.PositionsCard,
                //DashboardCardType.ExtraCostsCard,
            ],
        },
        InvoiceAccountingPosition: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [], //DashboardCardType.AccountingCard],
        },
        InvoicePosition: {
            kind: "localAndRemote",
            customLocks: ["OpenPositions"],
            lockedCards: [
                //DashboardCardType.PositionsCard,
                //DashboardCardType.ExtraCostsCard,
            ],
        },
        SalesOrderPosition: {
            kind: "localAndRemote",
            customLocks: ["OpenPositions"],
            lockedCards: [
                //DashboardCardType.PositionsCard,
                //DashboardCardType.ExtraCostsCard,
            ],
        },
        OrderConfirmationPosition: {
            kind: "localAndRemote",
            customLocks: ["OpenPositions"],
            lockedCards: [
                //DashboardCardType.PositionsCard,
                //DashboardCardType.ExtraCostsCard,
            ],
        },
        DeliveryNotePosition: {
            kind: "localAndRemote",
            customLocks: ["OpenPositions"],
            lockedCards: [
                //DashboardCardType.PositionsCard,
                //DashboardCardType.ExtraCostsCard,
            ],
        },
        PaymentInformationEntry: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [], //DashboardCardType.BankDetailsCard],
        },
        BankAccountDetails: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [], //DashboardCardType.BankDetailsCard],
        },
        BankAccountRefWithReadonlyDetails: {
            kind: "localAndRemote",
            customLocks: [],
            lockedCards: [], //DashboardCardType.BankDetailsCard],
        },
    });

    return typeToFlagsMap.get(typeStr);
}