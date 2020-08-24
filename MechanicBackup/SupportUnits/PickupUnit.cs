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

            int locationIndex = Workshop.getNearestWorkshopIndex(player.Character.Position);
            Vector3 spawnLocation = Workshop.spawnLocations[locationIndex];
            spawnLocation = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(100f));
            float spawnHeading = Workshop.spawnHeadings[locationIndex];

            Vehicle mechVehicle = new Vehicle(Config.SpawnVehicleNamePickup, spawnLocation, spawnHeading);
            Ped driver = new Ped("s_f_y_cop_01", mechVehicle.GetOffsetPositionRight(-mechVehicle.Width - 1f), spawnHeading);
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

            driver.Tasks.EnterVehicle(mechVehicle, 10000, -1, EnterVehicleFlags.None).WaitForCompletion(30000);
            var task1 = driver.Tasks.DriveToPosition(player.Character.Position, 15f, VehicleDrivingFlags.Emergency);
            task1.WaitForCompletion(60000);
            if (task1.IsActive)
            {
                mechVehicle.SetPositionWithSnap(player.Character.GetOffsetPositionFront(-2f));
            }
            player.Character.Tasks.EnterVehicle(mechVehicle, 10000, -1, EnterVehicleFlags.None).WaitForCompletion(30000);
            player.Character.WarpIntoVehicle(mechVehicle, 0);
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
            while (player.Character.CurrentVehicle == mechVehicle)
            {
                GameFiber.Yield();
            }
            GameFiber.Sleep(1000);

            mechVehicle.IsSirenOn = false;
            GameFiber.Sleep(1000);
            driver.IsPersistent = false;
            mechVehicle.IsPersistent = false;

            return;
        }
    }
}
