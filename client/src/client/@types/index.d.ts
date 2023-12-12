declare global {
	interface PlayerMp {
		browserRouter: string;
		fps: number;
		browserInstance: BrowserMp;
		browserCurrentState: string;
		isListening: boolean;
		__attachments: Attachment;
		__attachmentObjects: any;
		__attMgrData: any;
		_nickName: string;
		guiState: boolean;
	}

	interface PedMp {
		corpseId: number
		corpseCharacterId: number
	}
}

export interface UserData {
	accountId: number,
	adminDuty: boolean,
	adminLevel: number,
	adminName: string,
	playerId: number,
	username: string,
	isFlying: boolean,
	isFrozen: boolean,
	adminEsp: boolean,
	adminPed: string,
	showAdminPed: boolean
}

export interface SubtractVector {
	x: number,
	y: number,
	z: number
}

export interface StreetData {
	getStreet: GetStreetNameAtCoordResult,
	zoneName: string,
	zoneTwo: string
}

export interface Hunger {
	hunger: number,
	water: number
}

export interface CharacterData {
	characterId: number,
	characterName: string,
	characterModel: CharacterModel,
	data: DbCharacter,
	voiceChatState: boolean,
	characterClothing: ClothingData
}

export interface DbCharacter {
    character_id: number;
    owner_id: number;
    character_name: string;
    position_x: number;
    position_y: number;
    position_z: number;
    last_login: Date;
    character_health: number;
    character_isbanned: number;
    money_amount: number;
    play_time_seconds: bigint;
    player_dimension: number;
    player_exp: bigint;
    injured_timer: number;
    voiceChatState?: boolean;
    characterModel?: CharacterModel;
}

export interface CharacterModel {
	rotation: string;
	firstHeadShape: string;
	secondHeadShape: string;
	firstSkinTone: string;
	secondSkinTone: string;
	headMix: string;
	skinMix: string;
	sex: boolean;
	noseWidth: string;
	noseLength: string;
	noseTip: string;
	browHeight: string;
	cheekBoneHeight: string;
	cheeksWidth: string;
	lips: string;
	jawHeight: string;
	chinPosition: string;
	chinShape: string;
	noseHeight: string;
	noseBridge: string;
	noseBridgeShift: string;
	browWidth: string;
	cheekBoneWidth: string;
	eyes: string;
	jawWidth: string;
	chinLength: string;
	chinWidth: string;
	neckWidth: string;
	eyeColour: string;
	blemishes: string;
	ageing: string;
	facialHairStyle: string;
	facialHairColour: string;
	chestHairStyle: string;
	hairStyle: string;
	hairColour: string;
	hairHighlights: string;
	eyebrowsStyle: string;
	eyebrowsColour: string;
	complexion: string;
	sunDamage: string;
	molesFreckles: string;
	blushStyle: string;
	makeup: string;
	lipstick: string;
	blushColour: string;
}

export interface Gui {
	playerId: number,
	direction: string,
	isFrozen: boolean,
	unix: number,
	zoneName: string,
	zoneNameTwo: string,
	fps: number,
	voiceMuted: boolean
}

export interface Flight {
	flying: boolean,
	f: number,
	w: number,
	h: number,
	l: number,
	point_distance: number
}

export interface VehicleMods {
    vehicle_mod_id: number;
    vehicle_owner_id: number;
    spoilers: number;
    front_bumper: number;
    rear_bumper: number;
    side_skirt: number;
    exhaust: number;
    frame: number;
    grille: number;
    hood: number;
    fender: number;
    right_fender: number;
    roof: number;
    engine: number;
    brakes: number;
    transmission: number;
    horns: number;
    suspension: number;
    armor: number;
    turbo: number;
    xenon: number;
    front_wheels: number;
	wheel_type: number;
    back_wheels: number;
    plate_holders: number;
    trim_design: number;
    ornaments: number;
    dial_design: number;
    steering_wheel: number;
    shift_lever: number;
    plaques: number;
    hydraulics: number;
    boost: number;
    window_tint: number;
	pearleascent: number,
	wheel_colour:number;
    livery: number;
    plate: number;
    colour_1: number;
    colour_2: number;
}

export interface VehicleData {
	vehicle_id: number,
	owner_id: number,
	engine_status: boolean,
	vehicle_name: string,
	vehicle_locked: boolean,
	vehicle_spawn_hash: number,
	numberplate: string,
	position_x: string,
	position_y: string,
	position_z: string,
	vehicle_doors: string[],
	vehicle_windows: string[],
	indicator_status: number,
	vehicle_siren: boolean,
	vehicle_fuel: number,
	vehicle_distance: number,
	vehicle_mods: VehicleMods
}

interface ModInfo {
	name: string;
	modNumber: number;
}

export interface BoneData {
	id: number,
	boneIndex: number,
	name: string,
	locked: boolean,
	bonePos: Vector3,
	raycast: RaycastResult,
	veh: VehicleMp,
	distance: number,
	pushTime: number
}

export interface CreationCam {
	angle: number,
	dist: number,
	height: number
}

export interface Minimap {
	width: number;
	height: number;
	scaleX: number;
	scaleY: number;
	leftX: number;
	bottomY: number;
	rightX: number | null;
	topY: number | null;
}

export interface Attachment {
	id: number,
	model: number,
	offset: number,
	rotation: number,
	boneName: string
}

export interface SpeedoData {
	vehicleSpeed: number,
	vehicleRpm: number,
	indicatorStatus: number,
	lockStatus: boolean,
	lightsStates: { lightsOn: boolean; highbeamsOn: boolean; },
	fuelLevel: number,
	vehicleMileage: number,
	metric: number,
	numberPlate: string,
	displayName: string,
	dbName: string
}

export interface Corpse {
	characterName: string,
	characterId: number,
	model: CharacterModel,
	clothes: ClothingData,
	corpseId: number,
	position: Vector3,
	unixCreated: number
}

export interface ClothingStore {
	position: Vector3;
	name: string;
	id: number;
	displayName: string;
}

export interface ClothingData {
	clothing_id: number;
	character_id: number;
	mask: number;
	mask_texture: number;
	torso: number;
	torso_texture: number;
	leg: number;
	leg_texture: number;
	bags: number;
	bag_texture: number;
	shoes: number;
	shoes_texture: number;
	access: number;
	access_texture: number;
	undershirt: number;
	undershirt_texture: number;
	armor: number;
	armor_texture: number;
	decals: number;
	decals_texture: number;
	top: number;
	top_texture: number;
  }