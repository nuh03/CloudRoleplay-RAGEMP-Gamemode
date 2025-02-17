export default class SpeedCameras {
	public static LocalPlayer: PlayerMp = mp.players.local;
	public static serverCameraTrigger: string = 'server:handleSpeedCamera';
    public static flashDuration: number = 200;

	constructor() {
		mp.events.add({
            "client:speedCameraTrigger": SpeedCameras.handleTriggerEvent,
            "client:handleCameraFlash": SpeedCameras.handleCameraFlash
        });
	}

    public static async handleCameraFlash(vehId: number, camPos_x: number, camPos_y: number, camPos_z: number) {
        if(vehId !== undefined) {
            let camPos: Vector3 = new mp.Vector3(camPos_x, camPos_y, camPos_z);

            if(!camPos) return;

            mp.game.audio.playSoundFromCoord(1, "Camera_Shoot", camPos.x, camPos.y, camPos.z, "Phone_Soundset_Franklin", false, 0, false);

            let takeFlashInterval = setInterval(() => {
                let targetVeh: VehicleMp = mp.vehicles.at(vehId);

                if(targetVeh) {
                    let destinationCoords: Vector3 = targetVeh.position;
                    let dirVector: Vector3 = destinationCoords.subtract(camPos);

                    mp.game.graphics.drawSpotLight(camPos.x, camPos.y, camPos.z, dirVector.x, dirVector.y, dirVector.z, 255, 255, 255, 100, 5, 2, 100, 10);
                }
            }, 0)

            await mp.game.waitAsync(300);

            clearInterval(takeFlashInterval);
        }
    }

	public static handleTriggerEvent() {
		if (SpeedCameras.LocalPlayer.vehicle && SpeedCameras.LocalPlayer.vehicle.getPedInSeat(-1) == SpeedCameras.LocalPlayer.handle) {
			mp.events.callRemote(SpeedCameras.serverCameraTrigger, SpeedCameras.LocalPlayer.vehicle.getSpeed());
		}
	}
}