<template>
    <div v-if="getUiStates.chatEnabled && getUiStates.guiEnabled" class="duration-300 "
        :class="!showing ? 'opacity-40' : ''">
        <div id="chat">
            <ul id="chat_messages">
                <li v-for="(item, i) in chatMessages" :key="i" v-html="item.toString()">
                </li>
            </ul>
            <input class="border-l-4 border-black/60 resize" v-show="inputFieldShowing" v-model="userText" ref="input"
                id="chat_msg" type="text" />
            <span v-if="inputFieldShowing && userText.length > 0">
                {{ userText.length > maxLen ? maxLen : userText.length }} / {{ maxLen }}
            </span>
        </div>
    </div>
</template>

<script>
import { mapGetters } from "vuex";
import { sendToServer } from "@/helpers";

export default {
    data() {
        return {
            userText: "",
            KEYBIND_T: 84,
            KEYBIND_ENTER: 13,
            KEYBIND_UPARR: 38,
            KEYBIND_DOWNARR: 40,
            inputFieldShowing: false,
            showing: true,
            chatMessages: [],
            playerMessages: [],
            chatIteration: -1,
            inactiveTimer: 15000,
            closeInterval: null,
            closeTimeout: null,
            maxLen: 200
        };
    },
    computed: {
        ...mapGetters({
            getUiStates: "getUiStates"
        })
    },
    watch: {
        inputFieldShowing(oldVal) {
            this.typingState(oldVal);
        },
        chatMessages() {
            if (this.$router.currentRoute.path == "/") {
               // window.mp.trigger("gui:toggleHudComplete", true);
            }
            this.setCloseInterval();
        }
    },
    methods: {
        keyDownListener(e) {
            if (this.$router.currentRoute.path != "/" || !this.getUiStates.chatEnabled || !this.getUiStates.guiEnabled || this.$store.state.uiStates.phoneState) return;

            if (e.keyCode == this.KEYBIND_T && !this.inputFieldShowing) {
                this.showing = true;

                this.inputFieldShowing = true;

                if (this.$refs.input) {
                    this.$nextTick().then(() => {
                        this.$refs.input.focus();
                        this.$refs.input.select();
                    });
                }

                e.preventDefault();
            }

            if (e.keyCode == this.KEYBIND_ENTER) {
                if (this.userText.length > this.maxLen) return;

                this.inputFieldShowing = false;
                this.chatIteration = -1;

                if (!this.userText.length) return;
                let text = this.userText;

                if (text.length > 0) {
                    text.charAt(0) == "/" ? this.invokeCommand(text) : this.invokeChatMessage(text);
                    this.playerMessages.push(this.userText);
                    this.chatIteration = 0;
                    this.userText = "";

                    this.setCloseInterval();
                }
            }

            if ((e.keyCode == this.KEYBIND_DOWNARR || e.keyCode == this.KEYBIND_UPARR) && this.inputFieldShowing) {

                if (e.keyCode == this.KEYBIND_UPARR) {

                    if ((this.chatIteration + 1) > this.playerMessages.length - 1) {
                        this.chatIteration = 0;
                    } else {
                        this.chatIteration++;
                    }

                    this.userText = this.playerMessages.slice().reverse()[this.chatIteration];
                }

                if (e.keyCode == this.KEYBIND_DOWNARR) {

                    if ((this.chatIteration - 1) < 0) {
                        this.chatIteration = 0;
                    } else {
                        this.chatIteration--;
                    }

                    this.userText = this.playerMessages.slice().reverse()[this.chatIteration];
                }

            }
        },
        clearChat() {
            this.chatMessages = [];
        },
        typingState(toggle) {
            if (window.mp) {
                window.mp.invoke("focus", toggle);

                sendToServer("server:togglePlayerTyping", toggle);
                window.mp.invoke("setTypingInChatState", toggle);
            }
        },
        setCloseInterval() {
            this.showing = true;
            clearInterval(this.closeInterval);
            clearTimeout(this.closeTimeout);

            this.closeTimeout = setTimeout(() => {
                this.closeInterval = setInterval(() => {
                    if (!this.inputFieldShowing) {
                        this.showing = false;
                    }
                }, this.inactiveTimer)
            }, 12000);
        },
        invokeChatMessage(message) {
            if (window.mp) {
                window.mp.invoke("chatMessage", message);
            }
        },
        invokeCommand(command) {
            if (window.mp) {
                command = command.substr(1);
                window.mp.invoke("command", command);
            }
        },
        push(text) {
            this.chatMessages.unshift(`<span class='text-gray-400 opacity-80 text-sm'>${this.getTimeFormatted()} </span> ` + text);
        },
        getTimeFormatted() {
            const timeNow = new Date();
            const h = timeNow.getHours();
            const m = timeNow.getMinutes();

            let addZero = (num) => {
                return num <= 9 ? "0" + num : num;
            }

            let formatted = `${addZero(h)}:${addZero(m)}`;
            return formatted;
        }
    },
    created() {
        if (window.mp) {
            const api = {
                "chat:push": this.push,
                "msg:send": this.push
            };

            for (const fn in api) {
                if (window.mp) {
                    window.mp.events.add(fn, api[fn]);
                }
            }
        }

        this.setCloseInterval();
        document.addEventListener("keydown", this.keyDownListener);
    }
};
</script>


<style scoped>
*,
body,
html {
    opacity: 1;
    padding: 0;
    margin: 0;
    font-family: Myriad Pro, Segoe UI, Verdana, sans-serif;
    font-weight: 510;
    font-size: 16px;
    background-color: transparent;
    user-select: none;
    -webkit-touch-callout: none;
    /* iOS Safari */
    -webkit-user-select: none;
    /* Safari, Chrome, Opera, Samsung */
    -khtml-user-select: none;
    /* Konqueror HTML */
    -moz-user-select: none;
    /* Firefox */
    -ms-user-select: none;
    /* Edge, IE */
    user-select: none;
    /* Modern browsers */
    outline: none;
}

#chat,
a,
body,
html {
    color: #fff;
}

body,
html {
    -webkit-font-smoothing: antialiased;
    overflow: hidden;
    -webkit-transition: all 0.4s;
    -webkit-user-select: none;
}

#chat {
    width: 800px;
    font-weight: 700;
    text-shadow: 0 0 5px #000000, 0 0 6px #000000;
    font-size: 16px;
    margin-left: 15px;
}

@media screen and (min-height: 1080px) {
    #chat {
        font-size: 12px !important;
        font-weight: 700;
    }
}

#chat ul#chat_messages {
    height: 300px;
    margin-top: 1vh;
    transform: rotate(180deg);
    padding: 10px 20px;
    list-style-type: none;
    overflow: auto;
}

#chat ul#chat_messages>li {
    transform: rotate(-180deg);
}

#chat input#chat_msg {
    background-color: rgba(0, 0, 0, 0.425);
    color: white;
    outline: none;
    width: 800px;
    height: 3.12em;
    padding: 0 0.5em 0 0.5em;
    margin-top: 0.5em;
}

.suggestionDropDown {
    padding: 0 0.5em 0 0.5em;
    background-color: rgba(0, 0, 0, 0.425);
    color: rgb(169, 167, 167);
    border-left: solid rgb(121, 121, 121) 3px;
}
</style>