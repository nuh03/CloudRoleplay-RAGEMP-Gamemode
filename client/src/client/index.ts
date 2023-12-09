import AdminSystem from "./AdminSystem/AdminSystem";
import PlayerAuthentication from "./Authentication/PlayerAuthentication";
import BrowserSystem from "./BrowserSystem/BrowserSystem";
import NameTags from "./Nametags/Nametags";
import AdminFly from "./AdminSystem/AdminFly";
import AdminEvents from "./AdminSystem/AdminEvents";
import SwitchCamera from "./Authentication/SwitchCamera";
import GuiSystem from "./BrowserSystem/GuiSystem";
import EnterVehicle from "./VehicleSystems/EnterVehicle";
import VehicleLocking from "./VehicleSystems/VehicleLocking";
import NotificationSystem from "./NotificationSystem/NotificationSystem";
import VehicleSystems from "./VehicleSystems/VehicleSystem";
import VehicleInteraction from "./VehicleSystems/VehicleInteraction";
import VehicleWindows from "./VehicleSystems/VehicleWindows";
import VehicleEngine from "./VehicleSystems/VehicleEngine";
import VehicleIndicators from "./VehicleSystems/VehicleIndication";
import VehicleSiren from "./VehicleSystems/VehicleSiren";
import CharacterSystem from "./Character/CharacterSystem";
import AntiCheat from "./AntiCheat/AntiCheatSystem";
import AdminEsp from "./AdminSystem/AdminEsp";
import VoiceSystem from "./VoiceChat/VoiceSystem";
import DeathSystem from "./DeathSystem/DeathSystem";
import VehicleSpeedo from "./VehicleSystems/VehicleSpeedo";
import VehicleFuel from "./VehicleSystems/VehicleFuel";
import WeaponSystem from "./weaponSystem/weaponSystem";
import Corpses from "./DeathSystem/Corpses";
import Afk from "./AntiCheat/Afk";

// initialize client classes.
new PlayerAuthentication();
new BrowserSystem();
new AdminSystem();
new NameTags();
new AdminFly();
new AdminEvents();
new SwitchCamera();
new GuiSystem();
new EnterVehicle();
new VehicleLocking();
new NotificationSystem();
new VehicleSystems();
new VehicleInteraction();
new VehicleWindows();
new VehicleEngine();
new VehicleIndicators();
new VehicleSiren();
new CharacterSystem();
new AntiCheat();
new AdminEsp();
new VoiceSystem();
new DeathSystem();
new VehicleSpeedo();
new VehicleFuel();
new WeaponSystem();
new Corpses();
new Afk();