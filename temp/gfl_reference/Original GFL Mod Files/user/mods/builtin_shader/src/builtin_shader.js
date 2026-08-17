"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class builtin_shader {
    postDBLoad(container) {
        //Logger
        const logger = container.resolve("WinstonLogger");
        logger.info("Loading: builtin_shader");
    }
}
module.exports = { mod: new builtin_shader() };
//# sourceMappingURL=builtin_shader.js.map