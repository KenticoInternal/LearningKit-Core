﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using CMS.DataProtection;
using CMS.Helpers;
using CMS.ContactManagement;
using LearningKitCore.Models.DataProtection.TrackingConsent;

namespace LearningKitCore.Controllers.DataProtection
{
    public class TrackingConsentController : Controller
    {
        //DocSection:Constructor
        private readonly IConsentAgreementService consentAgreementService;
        private readonly ICurrentCookieLevelProvider currentCookieLevelProvider;
        private readonly IConsentInfoProvider consentInfoProvider;

        public TrackingConsentController(IConsentAgreementService consentAgreementService,
                                     ICurrentCookieLevelProvider currentCookieLevelProvider,
                                     IConsentInfoProvider consentInfoProvider)
        {
            this.consentAgreementService = consentAgreementService;
            this.currentCookieLevelProvider = currentCookieLevelProvider;
            this.consentInfoProvider = consentInfoProvider;
        }
        //EndDocSection:Constructor

        //DocSection:DisplayConsent
        public ActionResult DisplayConsent()
        {
            // Gets the related tracking consent
            // Fill in the code name of the appropriate consent object in Xperience
            ConsentInfo consent = consentInfoProvider.Get("SampleTrackingConsent");

            // Gets the current contact
            ContactInfo currentContact = ContactManagementContext.GetCurrentContact();

            // Sets the default cookie level for contacts who have revoked the tracking consent
            // Required for scenarios where one contact uses multiple browsers
            if ((currentContact != null) && !consentAgreementService.IsAgreed(currentContact, consent))
            {
                var defaultCookieLevel = currentCookieLevelProvider.GetDefaultCookieLevel();
                currentCookieLevelProvider.SetCurrentCookieLevel(defaultCookieLevel);
            }

            var consentModel = new TrackingConsentViewModel
            {
                // Adds the consent's short text to the model
                ShortText = consent.GetConsentText("en-US").ShortText,

                // Checks whether the current contact has given an agreement for the tracking consent
                IsAgreed = (currentContact != null) && consentAgreementService.IsAgreed(currentContact, consent)
            };

            return View("Consent", consentModel);
        }
        //EndDocSection:DisplayConsent

        //DocSection:AcceptConsent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Accept()
        {
            // Gets the related tracking consent
            ConsentInfo consent = consentInfoProvider.Get("SampleTrackingConsent");

            // Sets the visitor's cookie level to 'All' (enables contact tracking)
            currentCookieLevelProvider.SetCurrentCookieLevel(CookieLevel.All);

            // Gets the current contact and creates a consent agreement
            ContactInfo currentContact = ContactManagementContext.GetCurrentContact();
            consentAgreementService.Agree(currentContact, consent);

            return RedirectToAction("DisplayConsent");
        }
        //EndDocSection:AcceptConsent

        //DocSection:RevokeConsent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Revoke()
        {
            // Gets the related tracking consent
            ConsentInfo consent = consentInfoProvider.Get("SampleTrackingConsent");

            // Gets the current contact and revokes the tracking consent agreement
            ContactInfo currentContact = ContactManagementContext.GetCurrentContact();
            consentAgreementService.Revoke(currentContact, consent);

            // Sets the visitor's cookie level to the site's default cookie level (disables contact tracking)
            int defaultCookieLevel = currentCookieLevelProvider.GetDefaultCookieLevel();
            currentCookieLevelProvider.SetCurrentCookieLevel(defaultCookieLevel);

            return RedirectToAction("DisplayConsent");
        }
        //EndDocSection:RevokeConsent
    }
}
