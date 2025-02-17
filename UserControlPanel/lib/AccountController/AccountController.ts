import { AccountSessionOtpData, IAccount } from "@/types";
import pool from "../mysqlDb";
import crypto from 'crypto';
import jwt from "jsonwebtoken";
import { NextApiRequest } from "next";
import axios from "axios";

export interface TokenData {
    id: number,
    ip: string,
    adminLevel: number,
    adminName: string
};

export default class AccountsController {
    public static checkAccountPassword(targetHash: string, compare: string): boolean {
        let match: boolean = true;

        const hashBytes: Buffer = Buffer.from(targetHash, 'base64');
        const salt: Buffer = hashBytes.slice(0, 16);
        const pbkdf2: Buffer = crypto.pbkdf2Sync(compare, salt, 100000, 20, 'sha1');
        const hash: Buffer = pbkdf2;

        for (let i = 0; i < 20; i++) {
            if (hashBytes[i + 16] !== hash[i]) {
                match = false;
                break;
            }
        }

        return match;
    }

    public static tokenAuthentication(token: string | void | null | string[]): boolean {
        try {
            let decoded = jwt.verify(token as string, "jwtPrivateKey");

            decoded = decoded as {
                x: number
            };

            return decoded.id != -1 ? true : false;
        } catch (e) {
            return false;
        }
    }

    public static async confirmRecaptcha(response: string) {
        let res = await axios.post(`https://www.google.com/recaptcha/api/siteverify?secret=${process.env.RECAPTCHA_SECRET_KEY}&response=${response}`);

        return res.data.success;
    }

    public static otpVerification(token: string | void | null | string[]): AccountSessionOtpData | null {
        try {
            let decoded = jwt.verify(token as string, "jwtPrivateKey");

            decoded = decoded as AccountSessionOtpData;

            return decoded.accountOtpSession;
        } catch (e) {
            return null;
        }
    }

    public static getDataFromToken(req: NextApiRequest): TokenData | null {
        let accountId: number = -1;
        let decoded = jwt.verify(req.headers['x-auth-token'] as string, "jwtPrivateKey");

        if (decoded) {
            decoded = decoded as TokenData;
        }

        return decoded ? decoded as TokenData : null;
    }

    public static async postToBanHook(message: string) {
        const webhookUrl: string | undefined = process.env.DISCORD_BANS_WEBHOOK;

        if (!webhookUrl) {
            console.log("Discord webhook URL not found. Couldn't post ban message");
            return;
        }

        try {
            await axios.post(webhookUrl, {
                content: message,
                username: "Cloud RP | Bans",
                avatar_url: 'https://i.imgur.com/PAeaKFH.png'
            });
        } catch (error) {
            console.log("There was an error whilst sending a discord webhook ban message");
        }
    }

    public static generateOtp(length: number = 5): string {
        let otp: string = "";
        let numbers: string = "1234567890";

        for (let i = 0; i < length; i++) {
            otp += numbers[Math.floor(Math.random() * numbers.length)];
        }

        return otp;
    }
}