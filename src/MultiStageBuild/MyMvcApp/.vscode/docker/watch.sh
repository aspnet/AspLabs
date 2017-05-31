#!/bin/sh

# Parse arguments
SOURCE="${1:-/.src}"
TARGET="${2:-$(pwd)}"

# Create watch directory
mkdir -p /.watch

# Change to source directory
cd "$SOURCE"

# Ignore files in .dockerignore
FIND_OPTS=
if [ -e .dockerignore ]; then
  echo -n > /.watch/find_opts
  cat .dockerignore | sed -e 's/\r//' -e '$s/$/\n/' | while read line; do
    # Trim whitespace
    line="$(echo $line)"
    # Ignore comments and pattern "."
    if [ -z "$line" -o \
         -n "$(echo $line | grep ^#)" -o \
         -n "$(echo $line | grep ^\\.$)" ]; then
      continue
    fi
    # Ignore exceptions for now; no way to implement with find
    if [ -n "$(echo $line | grep ^\!)" ]; then
      continue;
    fi
    # Skip patterns with wildcards since find behavior is different
    if [ -n "$(echo $line | grep \*)" -o \
         -n "$(echo $line | grep \?)" -o \
         -n "$(echo $line | grep \\[)" ]; then
      continue
    fi
    # Remove leading '/', if any
    line="$(echo $line | sed -e 's/^\///')"
    # Concatenate clause to find options
    echo $(cat /.watch/find_opts) -path \""./$line"\" -prune -o > /.watch/find_opts
  done
  FIND_OPTS="$FIND_OPTS $(cat /.watch/find_opts)"
  rm /.watch/find_opts
fi
FIND_ALL_OPTS="$FIND_OPTS -print"
FIND_NEWER_OPTS="$FIND_OPTS -newer /.watch/last -print"

# Initialize state
eval find $FIND_ALL_OPTS | sed 's/^\.\///' > /.watch/baseline
touch -t $(date +%Y%m%d%H%M.%S) /.watch/last

# Start watch loop
while true; do
  # Detect deleted files
  eval find $FIND_ALL_OPTS 2>/dev/null | sed 's/^\.\///' > /.watch/current
  diff -U0 /.watch/baseline /.watch/current | grep ^-[^-] | cut -c2- | while read deleted; do
    echo rm -rf \"$TARGET/$deleted\" >> /.watch/log
    rm -rf "$TARGET/$deleted"
  done
  mv /.watch/current /.watch/baseline

  # Detect updated and added files
  touch -t $(date +%Y%m%d%H%M.%S) /.watch/now
  sleep 1 # MUST be here due to second precision on Mac
  eval find $FIND_NEWER_OPTS 2>/dev/null | sed 's/^\.\///' | while read newer; do
    if [ -d "$newer" ]; then
      echo mkdir -p \"$TARGET/$newer\" >> /.watch/log
      mkdir -p "$TARGET/$newer"
    else
      diff "$TARGET/$newer" "$newer" > /dev/null 2>&1
      if [ $? != 0 ]; then
        DIR=$(dirname "$newer")
        echo mkdir -p \"$TARGET/$DIR\" >> /.watch/log
        mkdir -p "$TARGET/$DIR"
        echo cp \"$newer\" \"$TARGET/$newer\" >> /.watch/log
        cp "$newer" "$TARGET/$newer"
      fi
    fi
  done
  mv /.watch/now /.watch/last
done
