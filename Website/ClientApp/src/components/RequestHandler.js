
export async function get(query) {
    let res = retrieve(query);
    if (res) {
        return { status: 200, value: res };
    }
    else {
        let response = await fetch(query);
        if (response.status === 200) {
            let value = await response.json();
            await store(query, value);
            return { status: 200, value: value };
        }
        return response;
    }
}

async function store(query, value) {
    let now = new Date().getTime();
    sessionStorage.setItem(query, JSON.stringify({ time: now, value: value }));
    return true;
}

function retrieve(query) {
    let item = JSON.parse(sessionStorage.getItem(query));
    if (item) {
        let now = new Date().getTime();
        if (item.time + 60000 > now) // 1 mins
            return item.value;
    }
    return false;
}