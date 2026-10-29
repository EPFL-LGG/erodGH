#!/bin/sh
set -eu
HERE=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
cd "$HERE"
/usr/bin/shasum -a 256 -c SHA256SUMS >/dev/null
OS=$(/usr/bin/plutil -extract os raw -o - package.json)
ARCH=$(/usr/bin/plutil -extract architecture raw -o - package.json)
KIND=$(/usr/bin/plutil -extract kind raw -o - package.json)
ABI=$(/usr/bin/plutil -extract nativeAbi raw -o - package.json)
[ "$ABI" = 2 ] || { echo "This installer requires native ABI 2."; exit 1; }
ACTUAL=$(/usr/bin/uname -m)
[ "$ACTUAL" != x86_64 ] || ACTUAL=x64
[ "$OS" = macos ] && [ "$KIND" = plugin ] && [ "$ACTUAL" = "$ARCH" ] || {
  echo "Choose the ERod plugin package for this Mac's architecture."; exit 1;
}
echo "Close Rhino. In Grasshopper, use File > Special Folders > Components Folder."
LIBRARY=$(/usr/bin/osascript -e 'POSIX path of (choose folder with prompt "Select the Components folder opened by Grasshopper > File > Special Folders > Components Folder")')
LIBRARY=$(CDPATH= cd -- "$LIBRARY" && pwd -P)
DEST="$LIBRARY/ERod"
case "$DEST/" in "$HERE/"*) echo "Choose the Grasshopper Components folder, not the extracted download."; exit 1;; esac
[ ! -L "$DEST" ] || { echo "The ERod installation cannot be a symbolic link."; exit 1; }
if [ -e "$DEST" ]; then
  BACKUP="$HOME/Library/Application Support/ERod/Backups/$(date -u +%Y%m%d-%H%M%S)-$$"
  mkdir -p "$(dirname "$BACKUP")"
  mv "$DEST" "$BACKUP"
  echo "Previous installation saved in $BACKUP"
fi
mkdir -p "$DEST"
cp -R "$HERE/." "$DEST/"
echo "ERod installed. Restart Rhino and Grasshopper."
