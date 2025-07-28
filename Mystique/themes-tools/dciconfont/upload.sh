#!/bin/bash
host='192.168.102.167'
user='cdn'
passwd='dc123456'
updir=/home/ci_dciconfont/dciconfont/$1
todir=./iconfont/$1
rmdir $todir
dirs=`find $updir -type d -printf $todir/'%P\n'| awk '{if ($0 == "")next;print "mkdir " $0}'`
files=`find $updir -type f -printf 'put %p %P \n'`
ftp  -nv  $host <<EOF
user ${user} ${passwd}
type binary
$dirs
cd $todir
$files
quit
EOF