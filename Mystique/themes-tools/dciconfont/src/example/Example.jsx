import React, { useState } from 'react';
import * as DcIcon from '@/Icons/index.jsx';
// import * as DcIcon from '../../dist/index';
import IconCard from './iconCard.jsx';
import './index.css';

// import CanvasBorderTest from "./CanvasBorderTest.jsx";

const Index = () => {
  const icons = Object.keys(DcIcon) || [];
  const [selIcon, setSelIcon] = useState();
  return (
    <div className="dcIconContainer">
      {icons.map((icon) => {
        const ChildComponent = DcIcon[icon];
        return (
          <IconCard
            key={icon}
            icon={<ChildComponent iconSize={32} />}
            title={icon}
            selIcon={selIcon}
            callback={(e) => {
              console.log(e);
              setSelIcon(e);
            }}
          />
        );
      })}
    </div>
  );
};

export default Index;
