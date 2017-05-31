#!/bin/sh
/bin/sh "$(dirname $0)/watch.sh" &
exec "$@"
