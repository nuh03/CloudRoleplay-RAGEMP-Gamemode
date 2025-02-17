import validateKeyPress from "@/PlayerMethods/validateKeyPress";
import { VehicleData } from "../../@types";
import { _SHARED_VEHICLE_DATA, _control_ids } from "../../Constants/Constants";
import getVehicleData from "../../PlayerMethods/getVehicleData";
import HandsUp from "@/Animation/HandsUpAnim";
import VehicleStall from "../Events/VehicleStall";

export default class VehicleEngine {
	public static LocalPlayer: PlayerMp = mp.players.local;
	public static readonly handleEngineStart: string = "server:handleEngineToggle";

	constructor() {
		mp.events.add("playerReady", VehicleEngine.handleStartUp);
		mp.events.add("playerEnterVehicle", VehicleEngine.handleEnter);
		mp.events.add("entityStreamIn", VehicleEngine.handleStreamIn);

		mp.events.addDataHandler(_SHARED_VEHICLE_DATA, VehicleEngine.handleDataHandler);

		mp.keys.bind(_control_ids.Y, false, () => validateKeyPress() && mp.events.callRemote(VehicleEngine.handleEngineStart));
	}

	public static handleStartUp() {
		mp.game.vehicle.defaultEngineBehaviour = false;
	}

	public static async handleStreamIn(entity: VehicleMp) {
		let vehicleData: VehicleData | undefined = getVehicleData(entity);
		if(entity.type == "vehicle" && vehicleData && vehicleData.engine_status && vehicleData.vehicle_fuel > 0) {

			for (let i = 0; entity.handle === 0 && i < 15; ++i) {
				await mp.game.waitAsync(100);
			}

			entity.setEngineOn(true, true, true);
		}
	}

	public static handleEnter(entity: VehicleMp) {
		if(getVehicleData(entity)?.engine_status) {
			entity.setEngineOn(true, true, true);
		}
	}

	public static handleDataHandler(entity: VehicleMp, vehicleData: VehicleData) {
		if(entity.type != "vehicle" || !vehicleData) return;

		if(mp.players.atHandle(entity.getPedInSeat(-1)) && mp.players.atHandle(entity.getPedInSeat(-1)).getVariable(HandsUp._handsUpAnimIdentifer)) return;

		if(vehicleData.engine_status && vehicleData.vehicle_fuel > 0 && vehicleData.vehicle_health > 0 && !entity.getVariable(VehicleStall.isStalledKey)) {
			entity.setEngineOn(true, true, true);
		} else {
			entity.setEngineOn(false, true, true);
			entity.setUndriveable(true);
		}
	}
}