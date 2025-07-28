#!/bin/bash

file_name=$1
file_path=$2
ftp_user=ftp
ftp_passwd=dctest123

echo "ftp put file '"$file_name"' begin" 
ftp -v -n 192.168.102.29<<EOF #登录ftp服务器
user ftp dctest123 #输入用户名密码
binary
quote pasv
passive
cd /project/xmen/$file_path/build/edge #FTP下载目录    /project/xmen/build/develop/cloud/     /project/autobots/cloud/$file_path/
lcd $(pwd)    #本地目录
prompt
mput $file_name #下载目录下所有文件
bye
EOF
echo "ftp put file '"$file_name"' end." 