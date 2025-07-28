#!/bin/bash
# restart
echo "The system will restart soon......"
cd `dirname $0`
# restart #
nohup ./Magneto >/dev/null 2>&1 &
