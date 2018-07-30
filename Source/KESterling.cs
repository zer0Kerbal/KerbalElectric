using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleKELights
{
    class KESterlingEngine : PartModule
    {
        [KSPField]
        public string resourceName = null;

        [KSPField]
        public double resourceAmt = 0.001f;

        [KSPField]
        public bool actRad = false;

        [KSPField(guiActive = true, guiName = "Electric Charge")]
        public double transAmt = 0.0;

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
            {
                return;
            }
            double kw = Math.Abs(this.part.thermalRadiationFlux);
            Part rPart = KEFunctions.GetResourcePart(FlightGlobals.ActiveVessel, resourceName);
            if (rPart != null)
            {
                int id = KEFunctions.GetResourceID(rPart, resourceName);
                double rTotal = KEFunctions.GetVesselResourceAmount(FlightGlobals.ActiveVessel, resourceName);
                double rMax = KEFunctions.GetVesselResourceMax(FlightGlobals.ActiveVessel, resourceName);
                transAmt = resourceAmt * kw;
                if (actRad == true) // look for active radiator
                {
                    for (int i = this.part.Modules.Count - 1; i >= 0; --i)
                    {
                        PartModule M = this.part.Modules[i];
                        if (M is ModuleActiveRadiator)
                        {
                            if ((M as ModuleActiveRadiator).IsCooling)
                            {
                                rPart.TransferResource(id, transAmt);
                            }
                        }
                    }
                }
                else
                {
                    rPart.TransferResource(id, transAmt);
                }
            }
        }
    }
}
