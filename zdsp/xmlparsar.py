#!/usr/bin/python
import sys

from xml.etree.ElementTree import parse, Element
#print("hello")
doc = parse(sys.argv[2])
root = doc.getroot()


subroot = root.findall("./applicationSettings/Photon.LoadBalancing.MasterServer.MasterServerSettings/setting")
for child in subroot:
	if child.attrib["name"] == "MasterVersion":
		child[0].text = sys.argv[1]

doc.write('Photon.LoadBalancing.dll.config', xml_declaration=True)