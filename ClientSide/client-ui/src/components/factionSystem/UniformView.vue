<template>
    <main>
        <div class="fixed inset-0 w-full text-white text-lg">
            <div class="container flex items-center max-w-3xl mx-auto mt-52">
                <div class="flex justify-center w-full">
                    <div
                        class="rounded-xl text-white w-full colourBackground border-b-4 border-t-4 border-purple-400/50 shadow-2xl shadow-black border-gray-400/40 select-none">

                        <div class="relative w-full h-fit py-4 rounded-lg">
                            <h1 class="font-bold text-2xl pb-2 pl-4"><i
                                    class="fa-solid fa-list text-gray-300"></i> Duty Uniforms</h1>
                            <CloseButton />

                            <div class="ml-4 mr-4 border-b-4 border-gray-400/50">

                            </div>

                            <div class="overflow-x-hidden overflow-y-scroll max-h-[30vw]">
                                <div v-for="(item, idx) in playerData.faction_uniforms" :key="idx"
                                    class="relative border mt-4 ml-2 mr-4 rounded-lg border-gray-400/40">


                                    <div class="p-4 h-48">
                                        <div class="absolute left-8">
                                            <div class="font-medium text-3xl">
                                                {{ item.uniformName }}
                                            </div>

                                        </div>
                                    </div>

                                    <div class="absolute w-full bottom-0">
                                        <div class="flex justify-center">
                                            <button @click="startDuty(item.uniformId)"
                                                class="w-full w-[40%] p-1.5 bg-black/40 rounded-lg border-gray-400/40 duration-300 hover:text-green-400 hover:border-green-400">
                                                <i class="fa-solid fa-play"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>

                            </div>

                            <div class="flex justify-center mt-4 p-3 font-medium">

                                <button @click="endDuty"
                                    class="border-2 w-full h-10 border-gray-400/50 hover:border-red-400 duration-300 rounded-lg">
                                    Go off duty
                                </button>

                            </div>

                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
import CloseButton from '../ui/CloseButton.vue';
import { mapGetters } from 'vuex';
import { sendToServer } from '@/helpers';

export default {
    components: {
        CloseButton
    },
    computed: {
        ...mapGetters({
            playerData: 'getPlayerInfo'
        })
    },
    methods: {
        startDuty(id) {
            sendToServer("server:factionSystem:duty", id);
        },
        endDuty() {
            sendToServer("server:factionSystem:dutyEnd", "");
        }
    }
}
</script>