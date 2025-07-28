import adsbImage from "../../assets/adsb.png";
import adsbImageSel from "../../assets/adsb_sel.png";
import fighterImage from "../../assets/fighter.png";
import fighterImageSel from "../../assets/fighter_sel.png";
import uavImage from "../../assets/uav.png";
import uavImageSel from "../../assets/uav_sel.png";
import unknownImage from "../../assets/unknown.png";
import unknownImageSel from "../../assets/unknown_sel.png";

import paraglidingImage from "../../assets/paragliding.png";
import paraglidingImageSel from "../../assets/paragliding_sel.png";

// import devCameraImage from "../../assets/dev_camera.png";
// import devControlImage from "../../assets/dev_control.png";
// import devMonirotImage from "../../assets/dev_monitor.png";
// import devRadarImage from "../../assets/dev_radar.png";
// import devDefindImage from "../../assets/dev_dfinding.png";
import monitoring_fault from "../../assets/monitoring_fault.png";
import monitoring_normal from "../../assets/monitoring_normal.png";

import radar_fault from "../../assets/radar_fault.png";
import radar_normal from "../../assets/radar_normal.png";

import recognizer_fault from "../../assets/recognizer_fault.png";
import recognizer_normal from "../../assets/recognizer_normal.png";

import radioSuppressing_fault from "../../assets/radioSuppressing_fault.png";
import radioSuppressing_normal from "../../assets/radioSuppressing_normal.png";

import directionFinding_fault from "../../assets/directionFinding_fault.png";
import directionFinding_normal from "../../assets/directionFinding_normal.png";

import uav_overview from "../../assets/uav_overview.png";
import pilot_overview from "../../assets/pilot_overview.png";
import landing_overview from "../../assets/landing_overview.png";

import pilot from "../../assets/uav_pilot.png";

const mapImageIds = {
  adsbImageId: "adsbImageId",
  adsbImageIdSel: "adsbImageId_sel",
  fighterImageId: "fighterImageId",
  fighterImageIdSel: "fighterImageId_sel",
  uavImageId: "uavImageId",
  uavImageIdSel: "uavImageId_sel",
  unknownImageId: "unknownImageId",
  unknownImageIdSel: "unknownImageId_sel",
  paraglidingId: "paragliding",
  paraglidingIdSel: "paragliding_sel",

  monitoring_fault: "monitoring_fault",
  monitoring_normal: "monitoring_normal",

  radar_fault: "radar_fault",
  radar_normal: "radar_normal",

  recognizer_normal: "recognizer_normal",
  recognizer_fault: "recognizer_fault",

  radioSuppressing_fault: "radioSuppressing_fault",
  radioSuppressing_normal: "radioSuppressing_normal",

  directionFinding_fault: "directionFinding_fault",
  directionFinding_normal: "directionFinding_normal",

  landing_overview: "landing_overview",
  pilot_overview: "pilot_overview",
  uav_overview: "uav_overview",

  pilot_image: "pilot_image",
};

const loadImages = (map) => {
  [
    {
      url: adsbImage,
      id: mapImageIds.adsbImageId,
    },
    {
      url: adsbImageSel,
      id: mapImageIds.adsbImageIdSel,
    },
    {
      url: fighterImage,
      id: mapImageIds.fighterImageId,
    },
    {
      url: fighterImageSel,
      id: mapImageIds.fighterImageIdSel,
    },
    {
      url: uavImage,
      id: mapImageIds.uavImageId,
    },
    {
      url: uavImageSel,
      id: mapImageIds.uavImageIdSel,
    },
    {
      url: unknownImage,
      id: mapImageIds.unknownImageId,
      sdf: false,
    },
    {
      url: unknownImageSel,
      id: mapImageIds.unknownImageIdSel,
    },
    {
      url: paraglidingImage,
      id: mapImageIds.paraglidingId,
    },
    {
      url: paraglidingImageSel,
      id: mapImageIds.paraglidingIdSel,
    },
    { id: mapImageIds.monitoring_normal, url: monitoring_normal },
    { id: mapImageIds.monitoring_fault, url: monitoring_fault },

    { id: mapImageIds.radar_normal, url: radar_normal },
    { id: mapImageIds.radar_fault, url: radar_fault },

    { id: mapImageIds.recognizer_normal, url: recognizer_normal },
    { id: mapImageIds.recognizer_fault, url: recognizer_fault },

    { id: mapImageIds.radioSuppressing_fault, url: radioSuppressing_fault },
    { id: mapImageIds.radioSuppressing_normal, url: radioSuppressing_normal },

    { id: mapImageIds.directionFinding_normal, url: directionFinding_normal },
    { id: mapImageIds.directionFinding_fault, url: directionFinding_fault },

    { id: mapImageIds.uav_overview, url: uav_overview },
    { id: mapImageIds.pilot_overview, url: pilot_overview },
    { id: mapImageIds.landing_overview, url: landing_overview },
    { id: mapImageIds.pilot_image, url: pilot },
  ].forEach((item) => {
    map.loadImage(item.url, (er, image) => {
      if (er) {
        console.log(`Load [${item.id}] error`, er);
        return;
      }
      map.addImage(item.id, image, { sdf: item.sdf });
    });
  });
};

export default loadImages;
export { mapImageIds };
