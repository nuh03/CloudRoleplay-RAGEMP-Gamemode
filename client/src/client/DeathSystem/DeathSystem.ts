import getUserCharacterData from "@/PlayerMethods/getUserCharacterData";
import { CharacterData } from "@/@types";
import { _sharedCharacterDataIdentifier } from "@/Constants/Constants";
import getTargetCharacterData from "@/PlayerMethods/getTargetCharacterData";
import ScaleForm from "@/Scaleform/ScaleformMessages";
import GuiSystem from "@/BrowserSystem/GuiSystem";
import VehicleSystems from "@/VehicleSystems/VehicleSystem";
import WeaponSystem from "@/WeaponSystem/WeaponSystem";

class DeathSystem {
    public static LocalPlayer: PlayerMp;
    public static _injuredIntervalUpdate_seconds: number = 10;
    public static _animCheck_seconds: number = 5;
    public static _injuredInterval: ReturnType<typeof setInterval> | undefined;
    public static _saveInterval: ReturnType<typeof setInterval> | undefined;
    public static injuredTimer: number;
    public static saveInjuredEvent: string = "server:saveInjuredTime";
    public static injuredAnim: string = "combat@damage@writheidle_a";
    public static _lib_injuredAnim: string = "writhe_idle_a";

    constructor() {
        DeathSystem.LocalPlayer = mp.players.local;

        mp.events.add("render", DeathSystem.handleRender);
        mp.events.add("entityStreamIn", DeathSystem.handleStreamIn);
        mp.events.addDataHandler(_sharedCharacterDataIdentifier, DeathSystem.handleDataHandler);
        mp.events.add("injured:startInterval", DeathSystem.handleIntervalStart);
        mp.events.add("injured:removeStatus", DeathSystem.removeIntervalStatus);

        setInterval(() => {
            mp.players.forEach(player => {
                let targetCharData: CharacterData | undefined = getTargetCharacterData(player);

                if(targetCharData && targetCharData.injured_timer > 0) {
                    DeathSystem.playDeathAnim(player);
                }
            })
        }, DeathSystem._animCheck_seconds * 1000);
    }

    public static removeIntervalStatus() {
        if(DeathSystem._injuredInterval) {
            clearInterval(DeathSystem._injuredInterval);
            DeathSystem._injuredInterval = undefined;
        }

        DeathSystem.LocalPlayer.freezePosition(false);
    }

    public static handleIntervalStart(time: number) {
        DeathSystem.playInjuredEffects();
        DeathSystem.turnGuiOnAfterScaleform();
        DeathSystem.LocalPlayer.freezePosition(true);
        DeathSystem.injuredTimer = time;

        DeathSystem._saveInterval = setInterval(() => {
            let characterData: CharacterData | undefined = getUserCharacterData();
            if(!characterData) return;

            characterData.injured_timer <= 0 && DeathSystem._saveInterval ? (clearInterval(DeathSystem._saveInterval), DeathSystem._saveInterval = undefined) : DeathSystem.injuredTimer--;
        }, 1000);

        DeathSystem._injuredInterval = setInterval(() => {
            mp.events.callRemote(DeathSystem.saveInjuredEvent);
        }, DeathSystem._injuredIntervalUpdate_seconds * 1000);
    }

    public static handleRender() {
        mp.game.gameplay.setFadeOutAfterDeath(false);

        let characterData: CharacterData | undefined = getUserCharacterData();
        if(!characterData) return;

        if(characterData.injured_timer > 0) {
            DeathSystem.disableControls();
            WeaponSystem.disableGunShooting();
            DeathSystem.renderInjuredText();

            if(DeathSystem.LocalPlayer.vehicle && DeathSystem.LocalPlayer.vehicle.getPedInSeat(-1) == DeathSystem.LocalPlayer.handle) {
                VehicleSystems.disableControls();
                DeathSystem.LocalPlayer.vehicle.setUndriveable(true);
            }
        }
    }

    public static async playInjuredEffects() {
        mp.game.cam.setCamEffect(1);
        ScaleForm.showShardMessage("~r~INJURED~w~", "You were injured", "", 0);
        mp.game.graphics.startScreenEffect('DeathFailMichaelIn', 60000, true);
        mp.game.audio.playSoundFrontend(-1, "Bed", "WastedSounds", true);
    }

    public static async turnGuiOnAfterScaleform() {
        await mp.game.waitAsync(2500);

        if(ScaleForm.isActive()) {
            DeathSystem.turnGuiOnAfterScaleform();
        } else {
            GuiSystem.toggleHudComplete(true);
        }
    }

    public static handleStreamIn(entity: EntityMp) {
        let characterData: CharacterData | undefined = getUserCharacterData();
        if(entity.type != "player" || !characterData) return;

        if(characterData.injured_timer > 0) {
            DeathSystem.playDeathAnim(entity as PlayerMp);
        }
    }

    public static handleDataHandler(entity: PlayerMp, data: CharacterData) {
        if(entity.type != "player" || !data) return;

        if(entity.remoteId != DeathSystem.LocalPlayer.remoteId) return;

        if(data.injured_timer > 0) {
            DeathSystem.injuredTimer = data.injured_timer;
            DeathSystem.playDeathAnim(entity);
        } else {
            mp.game.graphics.stopAllScreenEffects();
        }
    }

    public static async playDeathAnim(player: PlayerMp) {
		for (let i = 0; player.handle === 0 && i < 15; ++i) {
			await mp.game.waitAsync(100);
		}

        mp.game.streaming.requestAnimDict(DeathSystem.injuredAnim);

        await mp.game.waitAsync(50);

        player.taskPlayAnim(DeathSystem.injuredAnim, DeathSystem._lib_injuredAnim, 8.0, 1.0, -1, 1, 1.0, false, false, false);
    }

    public static disableControls() {
        mp.game.controls.disableControlAction(0, 22, true) //Space
        mp.game.controls.disableControlAction(0, 23, true) //Veh Enter
        mp.game.controls.disableControlAction(0, 25, true) //Right Mouse
        mp.game.controls.disableControlAction(0, 44, true) //Q
        mp.game.controls.disableControlAction(2, 75, true) //Exit Vehicle
        mp.game.controls.disableControlAction(2, 140, true) //R
        mp.game.controls.disableControlAction(2, 141, true) //Left Mouse
        mp.game.controls.disableControlAction(0, 30, true) //Move LR
        mp.game.controls.disableControlAction(0, 31, true) //Move UD
    }

    public static renderInjuredText() {
        if(!ScaleForm.isActive()) {
            mp.game.graphics.drawText(`~r~INJURED`, [0.5, 0.81], {
                font: 4,
                color: [255, 255, 255, 255],
                scale: [0.6, 0.6],
                outline: true
            });
            mp.game.graphics.drawText(`You will bleed out in ~HUD_COLOUR_ORANGE~${DeathSystem.injuredTimer}~w~ seconds.`, [0.5, 0.85], {
                font: 4,
                color: [255, 255, 255, 255],
                scale: [0.42, 0.42],
                outline: true
            });
        }
    }

}

export default DeathSystem;
