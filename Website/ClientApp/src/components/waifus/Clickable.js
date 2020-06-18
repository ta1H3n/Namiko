import React from 'react';
import { Modal } from 'react-bootstrap';

export function Clicklable(props) {
    const [show, setShow] = React.useState(false);

    const handleClose = () => setShow(false);
    const handleShow = () => setShow(true);

    return (
        <>
            <a href={window.location.pathname + "#" + props.waifu.name} className="stretched-link" onClick={handleShow} />
            <Modal show={show} onHide={handleClose} size="md" aria-labelbody="contained-modal-title-vcenter">
                <Modal.Header closeButton>
                    <Modal.Title>{props.waifu.longName}</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <div className="img_container">
                        <h5>{props.waifu.source}</h5>
                        <p>{props.waifu.description}</p>
                        <img src={props.waifu.imageUrl} className="img-fluid" />
                    </div>
                </Modal.Body>
            </Modal>
        </>
    );
}
