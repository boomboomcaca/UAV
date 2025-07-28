import React, { useState } from "react";
import PropTypes from "prop-types";
import { Tooltip } from "react-tooltip";
import styles from "./style.module.less";

const Snapshot = (props) => {
  const { className } = props;
  return (
    <svg
      width="32"
      height="32"
      viewBox="0 0 32 32"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <rect
        className={className}
        width="32"
        height="32"
        rx="2"
        fill-opacity="0.5"
      />
      <path
        d="M24.4 8.52496H22.4V8.5239C22.3996 8.11975 22.2308 7.73227 21.9306 7.44655C21.6305 7.16083 21.2235 7.00023 20.7991 7H18.4C17.9758 7.00045 17.5691 7.16113 17.2692 7.44678C16.9692 7.73242 16.8005 8.11972 16.8 8.52369V8.52475H7.6C7.17576 8.5252 6.76903 8.68591 6.46907 8.9716C6.16911 9.2573 6.00041 9.64465 6 10.0487V21.4763C6.00047 21.8803 6.1692 22.2676 6.46915 22.5532C6.76911 22.8389 7.1758 22.9995 7.6 23H24.4C24.8242 22.9995 25.2309 22.8389 25.5308 22.5532C25.8308 22.2676 25.9995 21.8803 26 21.4763V10.0487C25.9995 9.64468 25.8308 9.25739 25.5308 8.97174C25.2309 8.68609 24.8242 8.52541 24.4 8.52496ZM10.2 12.7138C10.0022 12.7138 9.80888 12.658 9.64443 12.5533C9.47998 12.4487 9.35181 12.3 9.27612 12.126C9.20043 11.9519 9.18063 11.7605 9.21921 11.5757C9.2578 11.391 9.35304 11.2213 9.49289 11.0881C9.63275 10.955 9.81093 10.8643 10.0049 10.8275C10.1989 10.7908 10.4 10.8096 10.5827 10.8817C10.7654 10.9538 10.9216 11.0758 11.0315 11.2325C11.1414 11.3891 11.2 11.5732 11.2 11.7615C11.2 12.0141 11.0946 12.2563 10.9071 12.4349C10.7196 12.6135 10.4652 12.7138 10.2 12.7138ZM16.8 20.7132C14.1491 20.7132 12.0009 18.6666 12.0009 16.143C12.0009 13.6194 14.15 11.5728 16.8 11.5728C19.45 11.5728 21.5991 13.6194 21.5991 16.143C21.5991 18.6666 19.4509 20.7132 16.8 20.7132Z"
        fill="white"
      />
      <path
        d="M16.5 13C14.5699 13 13 14.3458 13 16C13 17.6542 14.5701 19 16.5 19C18.4299 19 20 17.6542 20 16C20 14.3458 18.4299 13 16.5 13ZM16.5 18.2941C15.9706 18.2941 15.4532 18.1596 15.013 17.9075C14.5729 17.6554 14.2298 17.2971 14.0273 16.8779C13.8247 16.4587 13.7717 15.9975 13.875 15.5524C13.9782 15.1074 14.2331 14.6987 14.6074 14.3778C14.9818 14.057 15.4587 13.8385 15.9778 13.75C16.497 13.6614 17.0352 13.7069 17.5242 13.8805C18.0133 14.0541 18.4313 14.3482 18.7254 14.7255C19.0195 15.1027 19.1765 15.5463 19.1765 16C19.1757 16.6082 18.8934 17.1914 18.3917 17.6214C17.8899 18.0515 17.2096 18.2934 16.5 18.2941Z"
        fill="white"
      />
    </svg>
  );
};

const RecIcon = (props) => {
  const { className } = props;
  return (
    <svg
      width="32"
      height="32"
      viewBox="0 0 32 32"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <g clip-path="url(#clip0_143_72338)">
        <rect
          className={className}
          width="32"
          height="32"
          rx="2"
          fill="#18406E"
          fill-opacity="0.5"
        />
        <path
          fill-rule="evenodd"
          clip-rule="evenodd"
          d="M6 9C4.89543 9 4 9.89543 4 11V21C4 22.1046 4.89543 23 6 23H21C22.1046 23 23 22.1046 23 21V11C23 9.89543 22.1046 9 21 9H6ZM16.5 16.866C17.1667 16.4811 17.1667 15.5189 16.5 15.134L12 12.5359C11.3333 12.151 10.5 12.6321 10.5 13.4019L10.5 18.5981C10.5 19.3679 11.3333 19.849 12 19.4641L16.5 16.866Z"
          fill="white"
        />
        <path
          d="M24 12L28.3143 10.2743C28.6427 10.1429 29 10.3848 29 10.7385V21.2615C29 21.6152 28.6427 21.8571 28.3143 21.7257L24 20V12Z"
          fill="white"
        />
      </g>
      <defs>
        <clipPath id="clip0_143_72338">
          <rect width="32" height="32" fill="white" />
        </clipPath>
      </defs>
    </svg>
  );
};
const VideoBar = (props) => {
  const { onItemClick } = props;
  const [recording, setRecording] = useState(false);
  const [loading, setLoading] = useState(false);
  return (
    <div className={styles.vtb}>
      {/* <IconButton
        className={styles.btn}
        loading={loading}
        onClick={() => {
          if (loading) return;
          setLoading(true);
          setTimeout(() => {
            setLoading(false);
          }, 1000);
          onItemClick({ action: "snap" });
        }}
      >
        {snapshot}
      </IconButton>
      <IconButton
        tag="start"
        //    className={styles.comp}
        //    visible={visible}
        checked={recording}
        //    loading={loading}
        onClick={() => {
          onItemClick({ action: "rec", value: !recording });
          setRecording(!recording);
        }}
      >
        {recIcon(recording ? "red" : "#f0f0f0")}
      </IconButton> */}

      <div className={styles.tip}>
        <span>按下鼠标左键拉框选择跟踪物体</span>
      </div>
      <div className={styles.buttons}>
        <div
          className={`${styles.button} ${loading && styles.disable}`}
          data-tooltip-id="my-tooltip981"
          data-tooltip-content="快照"
          data-tooltip-place="bottom"
          onClick={() => {
            if (loading) return;
            setLoading(true);
            setTimeout(() => {
              setLoading(false);
            }, 1000);
            onItemClick({ action: "snap" });
          }}
        >
          <Snapshot className={styles.icon} />
        </div>
        <div
          className={`${styles.button} ${recording && styles.check}`}
          data-tooltip-id="my-tooltip981"
          data-tooltip-content="录像"
          data-tooltip-place="bottom"
          onClick={() => {
            if (loading) return;
            setLoading(true);
            setTimeout(() => {
              setLoading(false);
            }, 250);
            onItemClick({ action: "rec", value: !recording });
            setRecording(!recording);
          }}
        >
          <RecIcon className={styles.icon} />
        </div>
      </div>

      <Tooltip id="my-tooltip981" />
    </div>
  );
};

VideoBar.defaultProps = {
  onItemClick: () => {},
};

VideoBar.propTypes = {
  onItemClick: PropTypes.func,
};

export default VideoBar;
