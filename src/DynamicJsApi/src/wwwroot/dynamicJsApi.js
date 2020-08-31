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

    // These values are a subset of those specified in .NET's System.Linq.Expressions.ExpressionType.
    // The criteria for which values are included is based on
    // https://docs.microsoft.com/en-us/dotnet/api/system.dynamic.dynamicobject.trybinaryoperation
    // and
    // https://docs.microsoft.com/en-us/dotnet/api/system.dynamic.dynamicobject.tryunaryoperation
    const Operation = {
        ADD: 0,
        AND: 2,
        DIVIDE: 12,
        EQUAL: 13,
        EXCLUSIVE_OR: 14,
        GREATER_THAN: 15,
        GREATER_THAN_OR_EQUAL: 16,
        LEFT_SHIFT: 19,
        LESS_THAN: 20,
        LESS_THAN_OR_EQUAL: 21,
        MODULO: 25,
        MULTIPLY: 26,
        NEGATE: 28,
        UNARY_PLUS: 29,
        NOT: 34,
        NOT_EQUAL: 35,
        OR: 36,
        RIGHT_SHIFT: 41,
        SUBTRACT: 42,
        DECREMENT: 49,
        INCREMENT: 54,
        ADD_ASSIGN: 63,
        AND_ASSIGN: 64,
        DIVIDE_ASSIGN: 65,
        EXCLUSIVE_OR_ASSIGN: 66,
        LEFT_SHIFT_ASSIGN: 67,
        MODULO_ASSIGN: 68,
        MULTIPLY_ASSIGN: 69,
        OR_ASSIGN: 70,
        RIGHT_SHIFT_ASSIGN: 72,
        SUBTRACT_ASSIGN: 73,
        ONES_COPMPLEMENT: 82,
        IS_TRUE: 83,
        IS_FALSE: 84,
    };

    const objectIdPropertyName = '__jsObjectId';
    const objectCacheByTreeId = {};

    function getOrCreateObjectCache(treeId) {
        let cache = objectCacheByTreeId[treeId];

        if (!cache) {
            cache = [window];
            objectCacheByTreeId[treeId] = cache;
        }

        return cache;
    }

    function evaluateBinaryExpression(e, target) {
        switch (e.operation) {
            case Operation.ADD:
                return target + e.arg;
            case Operation.AND:
                return target & e.arg;
            case Operation.DIVIDE:
                return target / e.arg;
            case Operation.EQUAL:
                return target === e.arg;
            case Operation.EXCLUSIVE_OR:
                return target ^ e.arg;
            case Operation.GREATER_THAN:
                return target > e.arg;
            case Operation.GREATER_THAN_OR_EQUAL:
                return target >= e.arg;
            case Operation.LEFT_SHIFT:
                return target << e.arg;
            case Operation.LESS_THAN:
                return target < e.arg;
            case Operation.LESS_THAN_OR_EQUAL:
                return target <= e.arg;
            case Operation.MODULO:
                return target % e.arg;
            case Operation.MULTIPLY:
                return target * e.arg;
            case Operation.NOT_EQUAL:
                return target !== e.arg;
            case Operation.OR:
                return target | e.arg;
            case Operation.RIGHT_SHIFT:
                return target >> e.arg;
            case Operation.SUBTRACT:
                return target - e.arg;
            case Operation.ADD_ASSIGN:
                return target += e.arg;
            case Operation.AND_ASSIGN:
                return target &= e.arg;
            case Operation.DIVIDE_ASSIGN:
                return target /= e.arg;
            case Operation.EXCLUSIVE_OR_ASSIGN:
                return target ^= e.arg;
            case Operation.LEFT_SHIFT_ASSIGN:
                return target <<= e.arg;
            case Operation.MODULO_ASSIGN:
                return target %= e.arg;
            case Operation.MULTIPLY_ASSIGN:
                return target *= e.arg;
            case Operation.OR_ASSIGN:
                return target |= e.arg;
            case Operation.RIGHT_SHIFT_ASSIGN:
                return target >>= e.arg;
            case Operation.SUBTRACT_ASSIGN:
                return target -= e.arg;
            default:
                throw new Error(`Unknown binary operation with ID ${e.operation}.`);
        }
    }

    function evaluateUnaryExpression(e, target) {
        switch (e.operation) {
            case Operation.NEGATE:
                return -target;
            case Operation.UNARY_PLUS:
                return +target;
            case Operation.NOT:
                return !target;
            case Operation.DECREMENT:
                return --target;
            case Operation.INCREMENT:
                return ++target;
            case Operation.ONES_COPMPLEMENT:
                return ~target;
            case Operation.IS_TRUE:
                return !!target;
            case Operation.IS_FALSE:
                return !target;
            default:
                throw new Error(`Unknown unary operation with ID ${e.operation}.`);
        }
    }

    function evaluateExpression(e, target) {
        switch (e.type) {
            case ExpressionType.PROPERTY:
                return target[e.name];
            case ExpressionType.METHOD:
                return target[e.name].apply(target, e.args);
            case ExpressionType.INVOCATION:
                return target.apply(null, e.args);
            case ExpressionType.INSTANTIATION:
                return e.value;
            case ExpressionType.ASSIGNMENT:
                return target[e.name] = e.value;
            case ExpressionType.BINARY:
                return evaluateBinaryExpression(e, target);
            case ExpressionType.UNARY:
                return evaluateUnaryExpression(e, target);
            default:
                throw new Error(`Unknown expression type with ID ${e.type}.`);
        }
    }

    function generateRevivals(root, revivalsByObjectId) {
        Object.entries(root).forEach(function ([key, value]) {
            if (value !== null && typeof value === 'object') {
                if (value.hasOwnProperty(objectIdPropertyName)) {
                    const objectId = value[objectIdPropertyName];
                    const revivals = revivalsByObjectId[objectId] || (revivalsByObjectId[objectId] = []);
                    revivals.push({
                        parent: root,
                        key,
                    });
                } else {
                    generateRevivals(value, revivalsByObjectId);
                }
            }
        });
    }

    function evaluate(treeId, targetObjectId, expressionList) {
        const cache = getOrCreateObjectCache(treeId);
        const revivalsByObjectId = {};

        expressionList.forEach(function (e) {
            generateRevivals(e, revivalsByObjectId);
        });

        expressionList.forEach(function (e) {
            const target = cache[e.targetObjectId];
            const result = evaluateExpression(e, target);

            const resultObjectId = cache.length;
            const revivals = revivalsByObjectId[resultObjectId];

            if (revivals) {
                revivals.forEach(function ({ parent, key }) {
                    parent[key] = result;
                });
            }

            cache[resultObjectId] = result;
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
