import GridLayout from "react-grid-layout";
import MetricCard from "./widgets/MetricCard";
import RevenueChart from "./widgets/RevenueChart";

export default function Dashboard({ widgets, data }) {
  return (
    <GridLayout layout={widgets.map(w=>({i:w.id,...w.layout}))} cols={12} rowHeight={80} width={1200}>
      {widgets.map(w => (
        <div key={w.id}>
          {w.type === "metric" && <MetricCard value={data[w.metric]} />}
          {w.type === "chart" && <RevenueChart data={data.trend} />}
        </div>
      ))}
    </GridLayout>
  );
}
