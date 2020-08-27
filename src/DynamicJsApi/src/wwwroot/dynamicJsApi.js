(function () {
    const ExpressionType = {
        PROPERTY: 0,
        METHOD: 1,
        INVOCATION: 2,
        INSTANTIATION: 3,
        ASSIGNMENT: 4,
        BINARY: 5,
        UNARY: 6,
    };

    const Operation = {
        EQUAL: 13,
        NOT_EQUAL: 35,
        IS_TRUE: 83,
        IS_FALSE: 84
    }

    const objectIdPropertyName = '__objectId';

    const objectCacheByTreeId = {};

    function getOrCreateObjectCache(treeId) {
        let cache = objectCacheByTreeId[treeId];

        if (!cache) {
            cache = [window];
            objectCacheByTreeId[treeId] = cache;
        }

        return cache;
    }

    function reviveArg(arg, cache) {
        if (arg.hasOwnProperty(objectIdPropertyName)) {
            return cache[arg[objectIdPropertyName]];
        }

        return arg;
    }

    function reviveArgs(args, cache) {
        return args.map(function (arg) {
            return reviveArg(arg, cache);
        });
    }

    function evaluateBinaryExpression(e, target, cache) {
        switch (e.operation) {
            case Operation.EQUAL:
                return target === reviveArg(e.arg, cache);
            case Operation.NOT_EQUAL:
                return target !== reviveArg(e.arg, cache);
            default:
                throw new Error('Unknown binary operation.');
        }
    }

    function evaluateUnaryExpression(e, target) {
        switch (e.operation) {
            case Operation.IS_TRUE:
                return !!target;
            case Operation.IS_FALSE:
                return !target;
            default:
                throw new Error('Unknown unary operation.');
        }
    }

    function evaluateExpression(e, target, cache) {
        switch (e.type) {
            case ExpressionType.PROPERTY:
                return target[e.name];
            case ExpressionType.METHOD:
                return target[e.name].apply(target, reviveArgs(e.args, cache));
            case ExpressionType.INVOCATION:
                return target.apply(null, reviveArgs(e.args, cache));
            case ExpressionType.INSTANTIATION:
                return e.value;
            case ExpressionType.ASSIGNMENT:
                return target[e.name] = e.value;
            case ExpressionType.BINARY:
                return evaluateBinaryExpression(e, target, cache);
            case ExpressionType.UNARY:
                return evaluateUnaryExpression(e, target);
            default:
                throw new Error('Unknown expression type.');
        }
    }

    function evaluate(treeId, targetObjectId, expressionChain) {
        const cache = getOrCreateObjectCache(treeId);

        expressionChain.forEach(e => {
            const target = cache[e.targetObjectId];
            cache[cache.length] = evaluateExpression(e, target, cache);
        });

        if (targetObjectId < 0) {
            // This signals that we don't expect a result, and the current state should be disposed.
            delete objectCacheByTreeId[treeId];
        } else {
            // Return the target object.
            return cache[targetObjectId];
        }
    }

    window.jsObject = {
        evaluate,
    };
})();
