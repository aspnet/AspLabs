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
            case Operation.EQUAL:
                return target === e.arg;
            case Operation.NOT_EQUAL:
                return target !== e.arg;
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
                throw new Error('Unknown expression type.');
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

    function evaluate(treeId, targetObjectId, expressionChain) {
        const cache = getOrCreateObjectCache(treeId);
        const revivalsByObjectId = {};

        expressionChain.forEach(function (e) {
            generateRevivals(e, revivalsByObjectId);
        });

        expressionChain.forEach(function (e) {
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
