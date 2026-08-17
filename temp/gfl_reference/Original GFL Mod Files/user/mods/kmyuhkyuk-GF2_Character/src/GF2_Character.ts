import { DependencyContainer } from "tsyringe";
import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";
import { DatabaseServer } from "@spt/servers/DatabaseServer";
import { ILogger } from "@spt/models/spt/utils/ILogger";
import { PreSptModLoader } from "@spt/loaders/PreSptModLoader";
import { ImporterUtil } from "@spt/utils/ImporterUtil";

class GF2_Character implements IPostDBLoadMod
{
    public postDBLoad(container: DependencyContainer): void
    { 
        //Logger
        const logger = container.resolve<ILogger>("WinstonLogger");
		
        logger.info("Loading: kmyuhkyuk-GF2_Character");

        //Server database
        const databaseServer = container.resolve<DatabaseServer>("DatabaseServer");
        const tables = databaseServer.getTables();
		const databaseImporter = container.resolve<ImporterUtil>("ImporterUtil");

        //New database
        const PreSptModLoader = container.resolve<PreSptModLoader>("PreSptModLoader");
        const db = databaseImporter.loadRecursive(`${PreSptModLoader.getModPath("kmyuhkyuk-GF2_Character")}db/`);

		//Add customization
		for (const skin in db.templates.customization) {
			tables.templates.customization[skin] = db.templates.customization[skin];
		}

		//Add character
		for (const ca in db.templates.character) {
			tables.templates.character.push(db.templates.character[ca]);
		}

		for (const su in db.templates.profiles) {
			tables.templates.profiles[su] = db.templates.profiles[su];
		}

		//Add locales to game
//		for (const lang in db.locales)
//			for (const item in db.locales[lang].templates)
//				tables.locales.global[lang].templates[item] = db.locales[lang].templates[item];

		//Add trader
		for (const trader in db.traders) {
			//Add suits
			for (const st in db.traders[trader].suits)
				tables.traders[trader].suits.push(db.traders[trader].suits[st]);
		}	
    }
}

module.exports = { mod: new GF2_Character() }