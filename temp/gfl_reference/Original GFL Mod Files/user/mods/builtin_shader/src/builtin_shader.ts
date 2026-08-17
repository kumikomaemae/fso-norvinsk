import { DependencyContainer } from "tsyringe";

import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";
import { ILogger } from "@spt/models/spt/utils/ILogger";

class builtin_shader implements IPostDBLoadMod
{
    public postDBLoad(container: DependencyContainer): void
    { 
        //Logger
        const logger = container.resolve<ILogger>("WinstonLogger");
		
        logger.info("Loading: builtin_shader");	
    }
}

module.exports = { mod: new builtin_shader() }