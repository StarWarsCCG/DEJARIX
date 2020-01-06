async function startAsync() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/trade-hub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    await connection.start();
    console.log("connected");
    const result = await connection.invoke("ReverseText", "REVERSE ME!");
    console.log("RESPONSE: " + result);
    await connection.stop();
}

startAsync();
