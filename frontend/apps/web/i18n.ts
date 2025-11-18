import i18n from "i18next";
import { initReactI18next } from "react-i18next";

i18n
    .use(initReactI18next)
    .init({
        lng: "en", // default language
        fallbackLng: "en",
        ns: ["TranslationNamespaceSetupGuide"], // namespaces
        defaultNS: "TranslationNamespaceSetupGuide",
        resources: {
            en: {
                TranslationNamespaceSetupGuide: {
                    CUSTOMER_USERS: {
                        tabs: {
                            Sponsor: "Sponsor",
                            ProjectManager: "Project Manager",
                            ITContacts: "IT Contacts",
                            ErpApplicationOwner: "ERP Application Owner",
                            KeyUsers: "Key Users",
                        },
                        columns: {
                            CustomerSponsor: "Customer Sponsor",
                            CustomerProjectManager: "Customer Project Manager",
                            EmailExpert: "Email Expert",
                            NetworkExpert: "Network Expert",
                            SsoExpert: "SSO Expert",
                            ErpApplicationOwner: "ERP Application Owner",
                            KeyUsers: "Key Users",
                        },
                        fields: {
                            CustomerSponsorDetails: "Details about the customer sponsor",
                            CustomerProjectManagerDetails: "Details about the project manager",
                            EmailExpertDetails: "Details about the email expert",
                            NetworkExpertDetails: "Details about the network expert",
                            SsoExpertDetails: "Details about the SSO expert",
                            ErpApplicationOwnerDetails: "Details about the ERP application owner",
                            KeyUsersDetails: "Details about the key users",
                        },
                    },
                }
            },
            pl: {
                TranslationNamespaceSetupGuide: {
                    CUSTOMER_USERS: {
                        tabs: {
                            Sponsor: "Sponsor",
                            ProjectManager: "Kierownik projektu",
                            ITContacts: "Kontakty IT",
                            ErpApplicationOwner: "Właściciel aplikacji ERP",
                            KeyUsers: "Kluczowi użytkownicy",
                        },
                        columns: {
                            CustomerSponsor: "Sponsor klienta",
                            CustomerProjectManager: "Kierownik projektu po stronie klienta",
                            EmailExpert: "Ekspert ds. e-mail",
                            NetworkExpert: "Ekspert ds. sieci",
                            SsoExpert: "Ekspert SSO",
                            ErpApplicationOwner: "Właściciel aplikacji ERP",
                            KeyUsers: "Kluczowi użytkownicy",
                        },
                        fields: {
                            CustomerSponsorDetails: "Szczegóły dotyczące sponsora klienta",
                            CustomerProjectManagerDetails: "Szczegóły dotyczące kierownika projektu po stronie klienta",
                            EmailExpertDetails: "Szczegóły dotyczące eksperta ds. e-mail",
                            NetworkExpertDetails: "Szczegóły dotyczące eksperta ds. sieci",
                            SsoExpertDetails: "Szczegóły dotyczące eksperta SSO",
                            ErpApplicationOwnerDetails: "Szczegóły dotyczące właściciela aplikacji ERP",
                            KeyUsersDetails: "Szczegóły dotyczące kluczowych użytkowników",
                        },
                    },
                },
            },
        },
    });

export default i18n;
