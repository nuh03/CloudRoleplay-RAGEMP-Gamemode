import useEndpoint from "@/lib/Fetcher/useEndpoint";
import { factionColours } from "@/sharedConstants";
import { Faction } from "@/types";

const FactionsPage = ({ factions }: { factions: Faction[] }) => {
    return (
        <div className="pb-10">

            <h2 className="text-left border-l-4 pl-3 text-xl bg-gray-500/20 w-fit pr-6 p-2 mt-10 border-purple-400/50 ">Factions</h2>
            <div className="backdrop-blur-lg border-l-4 border-purple-400/50 max-h-96 overflow-y-scroll">
                {
                    factions.length > 0 &&
                    <table className="w-full  overflow-x-scroll min-w-[500px]">
                        <tbody>
                            <tr className="border-b-4 border-gray-400/50">
                                <th className="pb-4 relative pt-4">Faction Name</th>
                                <th className="pb-4">Colour</th>
                                <th className="pb-4">Owner</th>
                            </tr>
                            {
                                factions.map((faction: Faction, idx: number) => (
                                    idx > 0 && <tr key={idx} className="pb-4 border-b-2 border-gray-400/50">
                                        <th className="pt-4 pb-4">{faction.faction_name.replace("_", " ")}</th>
                                        <th style={{ backgroundColor: factionColours[idx] }} className="w-20 rounded-xl"></th>
                                        <th className="max-w-[40px] whitespace-nowrap overflow-hidden text-ellipsis">{faction.owner_name}</th>
                                    </tr>
                                ))
                            }
                        </tbody>
                    </table>
                }
            </div>



        </div >
    )
};

export default FactionsPage;