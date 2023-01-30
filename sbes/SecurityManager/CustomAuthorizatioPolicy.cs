﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SecurityManager
{
    class CustomAuthorizatioPolicy : IAuthorizationPolicy
    {
        public CustomAuthorizatioPolicy()
        {
            Id = Guid.NewGuid().ToString();
        }

        public ClaimSet Issuer
        {
            get { return ClaimSet.System; }
        }
        public string Id
        {
            get;
        }

        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            if (!evaluationContext.Properties.TryGetValue("Identities", out object list))
            {
                return false;
            }

            IList<IIdentity> identities = list as IList<IIdentity>;
            if (list == null || identities.Count <= 0)
            {
                return false;
            }


            //TODO
            //WindowsIdentity windowsIdentity = identities[0] as WindowsIdentity;

            //try
            //{
            //    AuditClient.Instance().LogAuthenticationSuccess(Formatter.ParseName(windowsIdentity.Name));
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}

            //if everthing is okey create new item in dictionary which will be our custom principal
            evaluationContext.Properties["Principal"] =
                new CustomPrincipal((WindowsIdentity)identities[0]);
            return true;
        }
    }
}