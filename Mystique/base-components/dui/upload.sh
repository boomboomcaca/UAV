#!/bin/bash
host='192.168.102.167'
user='front'
passwd='dctest123'
updir=/home/ci_dui/dui/$1
todir=tools/dui/$1
rmdir $todir
dirs=`find $updir -type d -printf $todir/'%P\n'| awk '{if ($0 == "")next;print "mkdir " $0}'`
files=`find $updir -type f -printf 'put %p %P \n'`
# ftp  -nv  $host <<EOF
ftp -n -v <<EOF
open $host 30
user ${user} ${passwd}
type binary
$dirs
cd $todir
$files
quit
EOF