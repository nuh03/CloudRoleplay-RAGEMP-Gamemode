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
import EmergencySystem from "./VehicleSystems/EmergencySystem";
import CharacterSystem from "./Character/CharacterSystem";
import AntiCheat from "./AntiCheat/AntiCheatSystem";
import AdminEsp from "./AdminSystem/AdminEsp";
import VoiceSystem from "./VoiceChat/VoiceSystem";
import DeathSystem from "./DeathSystem/DeathSystem";
import VehicleSpeedo from "./VehicleSystems/VehicleSpeedo";
import VehicleFuel from "./VehicleSystems/VehicleFuel";
import WeaponSystem from "./WeaponSystems/weaponSystem";
import Corpses from "./DeathSystem/Corpses";
import Afk from "./AntiCheat/Afk";
import Clothing from "./Character/Clothing";
import VehicleCustoms from "./customs/VehicleCustoms";
import VehicleDealerShips from "./vehicleDealerships/VehicleDealerships";
import Tattoos from "./Character/Tattoos";
import HousingSystem from "./housing/HousingSystem";
import VehicleRefueling from "./RefuellingSystem/VehicleRefueling";
import MarkersAndLabels from "./MarkersAndLabels/MarkersAndLabels";
import PhoneSystem from "./PhoneSystem/PhoneSystem";
import VehicleDamage from "./VehicleSystems/VehicleDamage";
import VehicleRadar from "./PoliceRadar/VehicleRadar";
import HandsUp from "./Animation/HandsUpAnim";
import ScaleForm from "./Scaleform/ScaleformMessages";
import SpeedCameras from "./SpeedCameras/SpeedCameras";
import InventorySystem from "./InventorySystem/InventorySystem";
import ConvienceStores from "./ConvienceStores/ConvIenceStores";
import CruiseControl from "./VehicleSystems/VehicleCruiseControl";
import PlayerDealership from "./PlayerDealership/PlayerDealership";
import Crouching from "./Animation/Crouching";
import Weather from "./world/Weather";
import BusDriverJob from "./Jobs/BusDriverJob/BusJob";
import KeyPressActions from "./KeyPressActions/KeyPressActions";
import TrailerSync from "./VehicleSystems/TrailerSync";
import VehicleStall from "./VehicleSystems/VehicleStall";
import TruckerJob from "./Jobs/TruckerJob/TruckerJobs";
import AttachmentSync from "./PlayerSyncSystems/AttachmentSync";
import AnimationSync from "./PlayerSyncSystems/AnimationSync";
import FactionSystem from "./FactionSystem/FactionSystem";
import Barber from "./Barber/Barber";

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
new Clothing();
new VehicleCustoms();
new VehicleDealerShips();
new Tattoos();
new HousingSystem();
new VehicleRefueling();
new MarkersAndLabels();
new PhoneSystem();
new VehicleDamage();
new VehicleRadar();
new HandsUp();
new ScaleForm();
new SpeedCameras();
new InventorySystem();
new ConvienceStores();
new CruiseControl();
new PlayerDealership();
//new Crouching();
//new Weather();
//new BusDriverJob();
//new KeyPressActions();
//new TrailerSync();
new EmergencySystem();
new VehicleStall();
new TruckerJob();
new AttachmentSync();
new AnimationSync();
new FactionSystem();
new Barber();