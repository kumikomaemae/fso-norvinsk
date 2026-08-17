import { DependencyContainer } from "tsyringe";
import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";
import { DatabaseServer } from "@spt/servers/DatabaseServer";
import { ILogger } from "@spt/models/spt/utils/ILogger";
import { PreSptModLoader } from "@spt/loaders/PreSptModLoader";
import { ImporterUtil } from "@spt/utils/ImporterUtil";

class GF_Voice implements IPostDBLoadMod
{
    public postDBLoad(container: DependencyContainer): void
    { 
        //Logger
        const logger = container.resolve<ILogger>("WinstonLogger");
		
        logger.info("Loading: kmyuhkyuk-GF_Voice");

        //Server database
        const databaseServer = container.resolve<DatabaseServer>("DatabaseServer");
        const tables = databaseServer.getTables();
		const databaseImporter = container.resolve<ImporterUtil>("ImporterUtil");

        //New database
        const PreSptModLoader = container.resolve<PreSptModLoader>("PreSptModLoader");
        const db = databaseImporter.loadRecursive(`${PreSptModLoader.getModPath("kmyuhkyuk-GF_Voice")}db/`);

        //Add customization
		for (const voice in db.templates.customization) {
			tables.templates.customization[voice] = db.templates.customization[voice];
		}

		//Add globals to game
		for (const item in db.globals.config.Customization.CustomizationVoice) {
			tables.globals.config.Customization.CustomizationVoice[item] = db.globals.config.Customization.CustomizationVoice[item];
		}

		//Add locales to game
//		for (const lang in db.locales)
//			for (const item in db.locales[lang].templates)
//				tables.locales.global[lang].templates[item] = db.locales[lang].templates[item];
    }
}

module.exports = { mod: new GF_Voice() }