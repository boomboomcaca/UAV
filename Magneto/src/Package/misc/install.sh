#!/bin/bash
# install
echo "install the edge server as system service!"
cd `dirname $0`

chmod +x ./Magneto
echo "stop the edge server"
systemctl stop edge
# copy file
echo "copy the service file to system dir"
cp edge.service /usr/lib/systemd/system/
echo "start the edge service..."
systemctl daemon-reload
systemctl start edge.service
# #
systemctl enable edge.service