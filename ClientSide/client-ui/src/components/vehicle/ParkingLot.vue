<template>
    <main class="relative">
        <div class="absolute right-[3%] text-white">

            <div class="relative mt-52 colourBackground p-3 rounded-xl shadow-2xl shadow-black border-b-4 border-t-4 border-purple-400/50">
                <h1 class="font-bold text-2xl pl-4">
                    <i class="fa-solid fa-car text-gray-300"></i> Vehicle List
                </h1>
                <CloseButton />
            </div>

            <div class="container flex items-center w-[25vw] mx-auto mt-6">

                <div class="flex justify-center w-full">
                    <div class="rounded-xl w-full colourBackground border-t-4 border-b-4 border-purple-400/50 shadow-2xl shadow-black select-none">

                        <div class="relative w-full h-fit pb-2 rounded-lg">
                            <div class="max-h-[30vw] overflow-scroll overflow-x-hidden">
                                <div v-for="item in playerData.parked_vehicles" :key="item.key_uuid">
                                    <div class="border-t-2 w-full mt-6 relative p-6 border-b-2 border-gray-400/40">
                                        <div v-if="getCarImagePath(item.vehicle_name)" class="absolute right-3">
                                            <img :src="getCarImagePath(item.vehicle_name)" alt="Car Image"
                                                class="w-30 h-20 rounded-xl" />
                                        </div>
                                        <font class="font-bold text-xl">
                                            {{ item.vehicle_display_name }}
                                        </font>
                                        <br />
                                        <div class="relative mt-2">
                                            <div id="numberplate" style="text-shadow: rgba(0, 0, 0, 0.563) 1px 0 10px;"
                                                class='bg-gray-300/40 w-40 text-center text-3xl outline-none rounded-lg'>
                                                {{ item.numberplate }}
                                            </div>
                                        </div>
                                        <button @click="unparkVeh(item.vehicle_id)"
                                            class=" mt-2 bg-green-500/50 p-1 font-medium rounded-lg pr-4 pl-4 hover:bg-green-500/80 duration-300">
                                            Select
                                        </button>
                                        <div class="mt-3">
                                            <div v-if="item.vehicle_distance > 0"
                                                class="absolute right-4 bottom-1 font-medium text-gray-300">
                                                <p><i class="fa-solid fa-gauge"></i> {{ (item.vehicle_distance /
                                                    1000).toFixed(0) }}km </p>
                                            </div>
                                            <div v-if="item.vehicle_distance / 1609 > 0"
                                                class="absolute right-4 bottom-7 font-medium text-gray-300">
                                                <p><i class="fa-solid fa-gas-pump"></i> {{ item.vehicle_fuel.toFixed(0) }}%
                                                </p>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="p-3 font-medium text-gray-300 text-center" v-if="playerData.parked_vehicles == 0">
                                You don't have any vehicles parked here.
                            </div>

                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
import { mapGetters } from 'vuex';
import CloseButton from '../ui/CloseButton.vue';
import { sendToServer } from '@/helpers';

export default {
    components: {
        CloseButton
    },
    computed: {
        ...mapGetters({
            playerData: 'getPlayerInfo',
            uiStates: 'getUiStates',
            loadingState: 'getLoadingState'
        }),
    },
    methods: {
        getCarImagePath(dbName) {
            try {
                const imageModule = require(`../../assets/img/cars/${dbName}.png`);
                return imageModule;
            } catch (error) {
                return require("../../assets/img/cars/sentinel.png");
            }
        },
        unparkVeh(vehId) {
            sendToServer("server:unparkVehicle", vehId);
        }
    }
}
</script>