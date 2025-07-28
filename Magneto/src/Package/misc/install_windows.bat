sc create xmen_edge_main binpath= %~dp0Magneto.exe type= own start= auto displayname= xmen_edge_main
sc failure xmen_edge_main reset=0 actions=restart/3000