using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// using System.Threading.Tasks;

namespace ModuleKELights
{
    public static class KEFunctions
    {
        public static Part GetResourcePart(Vessel v, string resourceName)
        {
            foreach (Part mypart in v.parts)
            {
                if (mypart.Resources.Contains(resourceName))
                {
                    return mypart;
                }
            }
            return null;
        }

        public static int GetResourceID(this Part part, string resourceName)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceName);
            return resource.id;
        }

        public static double GetVesselResourceAmount(Vessel v, string resourceName)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceName);
            double amount = 0.0f;
            foreach (Part mypart in v.parts)
            {
                if (mypart.Resources.Contains(resourceName))
                {
                    amount += GetPartResourceAmount(mypart, resourceName);
                }
            }
            return amount;
        }

        public static double GetVesselResourceMax(Vessel v, string resourceName)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceName);
            double amount = 0.0f;
            foreach (Part mypart in v.parts)
            {
                if (mypart.Resources.Contains(resourceName))
                {
                    amount += GetPartResourceMax(mypart, resourceName);
                }
            }
            return amount;
        }

        public static double GetPartResourceAmount(this Part part, string resourceName)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceName);
            double amount = 0.0f;
            if (part.Resources.Contains(resource.id))
            {
                amount = part.Resources.Get(resource.id).amount;
            }
            return amount;
        }

        public static double GetPartResourceMax(this Part part, string resourceName)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceName);
            double amount = 0.0f;
            if (part.Resources.Contains(resource.id))
            {
                amount = part.Resources.Get(resource.id).maxAmount;
            }
            return amount;
        }
    }
}
