import React from 'react';
import { Modal, OverlayTrigger, Tooltip } from 'react-bootstrap';

export function Clickable(props) {
    const [show, setShow] = React.useState(false);

    const handleClose = () => setShow(false);
    const handleShow = () => setShow(true);

    return (
        <>
            <div className="stretched-link" onClick={handleShow} />
            <Modal show={show} onHide={handleClose} size="md">
                <Modal.Header className={"t" + ParseTier(props.waifu.tier)} closeButton>
                    <Modal.Title>{props.waifu.longName}</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <div className="img_container">
                        <h5>{props.waifu.source}</h5>
                        <p>{props.waifu.description}</p>
                        <div class="waifu-image">
                            <img src={props.waifu.imageRaw} alt={props.waifu.name} className="img-fluid" />
                            {SourcePop(props.waifu.imageSource)}
                        </div>
                    </div>
                </Modal.Body>
            </Modal>
        </>
    );
}

export function ParseTier(tier) {
    let t = tier < 1
        ? 0
        : tier > 3
            ? 0
            : tier;
    return t;
}

function SourcePop(source) {
    if (source === null || source === "retry" || source === "missing" || source === "custom" || source === "") {
        return (
            <OverlayTrigger overlay={<Tooltip id="source">Source missing! :(</Tooltip>}>
                <div class="btn waifu-image-btn">Source</div>
            </OverlayTrigger>);
    }

    return (<a href={source} target="_blank" rel="noopener noreferrer" class="btn waifu-image-btn">Source</a>);
}
