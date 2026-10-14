class Graph {
    // graph: adjacency list
    static dijkstra(graph: [number, number][][], start: number): number[] {
        let dist = Array<number>(graph.length).fill(Number.MAX_SAFE_INTEGER);
        dist[start] = 0;

        let visited = Array<boolean>(graph.length).fill(false);

        let pq = new Heap<number>((a, b) => dist[a] > dist[b]);
        pq.push(start);

        while (!pq.empty()) {
            let i = pq.top();
            pq.pop();

            if (visited[i]) continue;
            visited[i] = true;
            //console.log(`pop ${i}: ${dist[i]}`);

            for (let [j, len] of graph[i]) {
                if (!visited[j] && dist[i] + len < dist[j]) {
                    dist[j] = dist[i] + len;
                    pq.push(j);
                    //console.log(`push ${j}: ${dist[j]}`);
                }
            }
            //console.log(dist);
        }

        return dist;
    }
}