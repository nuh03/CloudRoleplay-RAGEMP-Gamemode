import { VehicleData } from "@/@types";
import getVehicleData from "@/PlayerMethods/getVehicleData";

export default class VehicleDamage {
    public static LocalPlayer: PlayerMp;
    public static readonly saveDamageEvent: string = "server:saveVehicleDamage";
    public static _updateInterval: ReturnType<typeof setInterval> | undefined;

    constructor() {
        VehicleDamage.LocalPlayer = mp.players.local;

        mp.events.add("playerEnterVehicle", (veh: VehicleMp) => VehicleDamage.handleEnterOrLeave(veh, true));
        mp.events.add("playerLeaveVehicle", VehicleDamage.handleEnterOrLeave);
    }

    public static async handleEnterOrLeave(vehicle: VehicleMp, isEnterEvent: boolean = false) {
        if(!vehicle) return;
        let vehicleData: VehicleData | undefined = getVehicleData(vehicle);

        if(vehicleData && vehicleData.vehicle_health !== undefined && typeof vehicleData.vehicle_health === "number") {
            vehicle.setHealth(vehicleData.vehicle_health);
            vehicle.setBodyHealth(vehicleData.vehicle_health);
            vehicle.setEngineHealth(vehicleData.vehicle_health);
        }

        await mp.game.waitAsync(2500);

        isEnterEvent ? VehicleDamage.startInterval() : VehicleDamage.closeInterval();
    }

    public static startInterval() {
        VehicleDamage._updateInterval = setInterval(async () => {
            if(VehicleDamage.LocalPlayer.vehicle) {
                let vehicleData: VehicleData | undefined = getVehicleData(VehicleDamage.LocalPlayer.vehicle);

                if(vehicleData && vehicleData.vehicle_health != Math.round(VehicleDamage.LocalPlayer.vehicle.getBodyHealth())) {
                    mp.events.callRemote(VehicleDamage.saveDamageEvent);
                    await mp.game.waitAsync(500);

                    let data: VehicleData | undefined = getVehicleData(VehicleDamage.LocalPlayer.vehicle);

                    if(VehicleDamage.LocalPlayer.vehicle && data && typeof data.vehicle_health == "number") {
                        VehicleDamage.LocalPlayer.vehicle.setEngineHealth(data.vehicle_health);
                    }
                }
            }
        }, 1000);
    }

    public static closeInterval() {
        if(VehicleDamage._updateInterval) {
            clearInterval(VehicleDamage._updateInterval);
            VehicleDamage._updateInterval = undefined;
        }
    }
}