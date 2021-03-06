﻿using Rage;
using System.Drawing;

namespace MechanicBackup.SupportUnits
{
    class PickupUnit
    {
        public static void spawn(Player player)
        {
            Blip waypoint = World.GetWaypointBlip();
            if (waypoint == null)
            {
                Game.DisplayNotification("Please set a waypoint first!");
                return;
            }

            int locationIndex = Stations.getNearestStationIndex(player.Character.Position);
            Vector3 spawnLocation = Stations.spawnLocations[locationIndex];
            //spawnLocation = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(100f));
            float spawnHeading = Stations.spawnHeadings[locationIndex];

            Vehicle mechVehicle = new Vehicle(Config.SpawnVehicleNamePickup, spawnLocation, spawnHeading);
            Ped driver = new Ped("s_f_y_cop_01", mechVehicle.GetOffsetPositionRight(-mechVehicle.Width /*- 1f*/), spawnHeading);
            Blip vehicleBlip = new Blip(mechVehicle);

            driver.BlockPermanentEvents = true;
            driver.IsPersistent = true;
            driver.IsInvincible = true;
            mechVehicle.IsPersistent = true;
            mechVehicle.IsSirenOn = true;
            mechVehicle.IsSirenSilent = false;
            vehicleBlip.Color = Color.Gray;
            vehicleBlip.IsFriendly = true;

            Game.DisplayNotification("Dispatching pickup unit");

            //driver.Tasks.EnterVehicle(mechVehicle, 10000, -1, EnterVehicleFlags.None).WaitForCompletion(30000);

            driver.WarpIntoVehicle(mechVehicle, -1);
            
            var task1 = driver.Tasks.DriveToPosition(World.GetNextPositionOnStreet(player.Character.Position), 15f, VehicleDrivingFlags.Emergency);
            //var task1 = driver.Tasks.DriveToPosition(player.Character.Position, 15f, VehicleDrivingFlags.Emergency);
            Game.DisplayHelp("Hold Back to warp the vehicles closer");

            while ((task1 != null && task1.IsActive))
            {
                GameFiber.Yield();
                if (Game.IsKeyDown(System.Windows.Forms.Keys.Back))
                {
                    GameFiber.StartNew(delegate
                    {
                        GameFiber.Sleep(2000);
                        if (Game.IsKeyDownRightNow(System.Windows.Forms.Keys.Back))
                        {
                            Game.DisplayHelp("Warping closer");
                            mechVehicle.Position = World.GetNextPositionOnStreet(player.Character.Position);
                            GameFiber.Sleep(1000);
                        }
                    });
                }
                if (mechVehicle.DistanceTo(player.Character.Position) < 500f)
                {
                    mechVehicle.IsSirenOn = true;
                    mechVehicle.IsSirenOn = true;
                }
            }

            if (player.Character.Position.DistanceTo(mechVehicle.Position) > 10f)
            {
                Game.DisplayNotification("Please get closer to the pickup vehicle");
                while(player.Character.Position.DistanceTo(mechVehicle.Position) > 10f)
                {
                    GameFiber.Yield();
                }
            }

            Game.DisplayNotification("Entering vehicle");
            player.Character.Tasks.EnterVehicle(mechVehicle, 0).WaitForCompletion(30000);
            //Game.LocalPlayer.Character.Tasks.EnterVehicle(mechVehicle, 10000, 0, EnterVehicleFlags.None).WaitForCompletion(30000);
            //player.Character.WarpIntoVehicle(mechVehicle, 0);
            while (player.Character.CurrentVehicle != mechVehicle)
            {
                GameFiber.Yield();
            }
            // wait for player to enter vehicle
            driver.Tasks.DriveToPosition(World.GetNextPositionOnStreet(waypoint.Position), 20f, VehicleDrivingFlags.Emergency).WaitForCompletion();
            vehicleBlip.Delete();

            mechVehicle.IsSirenOn = true;
            mechVehicle.IsSirenSilent = true;
            Game.DisplayNotification("You can leave the vehicle now!");
            while (player.Character.CurrentVehicle == mechVehicle || mechVehicle.DistanceTo(player.Character.Position) < 5)
            {
                GameFiber.Yield();
            }

            mechVehicle.IsSirenOn = false;
            GameFiber.Sleep(1000);
            driver.IsPersistent = false;
            mechVehicle.IsPersistent = false;

            return;
        }
    }
}
