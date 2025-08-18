import { useState } from "react";
import styles from "./styles.module.scss";

interface CustomCalendarProps {
  selectedDate: Date | null;
  onSelectDate: (date: Date | null) => void;
  onClose: () => void;
}

const CustomCalendar = ({
  selectedDate,
  onSelectDate,
  onClose,
}: CustomCalendarProps) => {
  const [currentDate, setCurrentDate] = useState(new Date());

  const startOfMonth = new Date(
    currentDate.getFullYear(),
    currentDate.getMonth(),
    1
  );
  const endOfMonth = new Date(
    currentDate.getFullYear(),
    currentDate.getMonth() + 1,
    0
  );
  const days = [];

  const startDay = startOfMonth.getDay();
  const totalDays = endOfMonth.getDate();

  for (let i = 0; i < startDay; i++) {
    days.push(null);
  }
  for (let i = 1; i <= totalDays; i++) {
    days.push(new Date(currentDate.getFullYear(), currentDate.getMonth(), i));
  }

  const handlePrev = () => {
    setCurrentDate(
      new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1)
    );
  };

  const handleNext = () => {
    setCurrentDate(
      new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1)
    );
  };

  return (
    <div className={styles["calendar-container"]}>
      <div className={styles["calendar-header"]}>
        <div className={styles["calendar-header-button"]} onClick={handlePrev}>
          ‹
        </div>
        <span>
          {currentDate.toLocaleString("default", { month: "long" })}{" "}
          {currentDate.getFullYear()}
        </span>
        <div className={styles["calendar-header-button"]} onClick={handleNext}>
          ›
        </div>
      </div>

      <div className={styles["calendar-grid"]}>
        {["S","M", "T", "W", "T", "F", "S"].map((day, index) => (
          <div key={day + index} className={styles["day-label"]}>
            {day}
          </div>
        ))}
        {days.map((date, index) => (
          <div key={index} className={styles["day-cell"]}>
            {date && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  e.preventDefault();
                  onSelectDate(date);
                }}
                className={
                  selectedDate?.toDateString() === date.toDateString()
                    ? styles["active-day"]
                    : ""
                }
              >
                {date.getDate()}
              </button>
            )}
          </div>
        ))}
      </div>

      <div className={styles["calendar-actions"]}>
        <button
          onClick={(e) => {
            e.preventDefault();
            onSelectDate(null);
            onClose();
          }}
        >
          Remove
        </button>
        <button onClick={onClose}>Done</button>
      </div>
    </div>
  );
};

export default CustomCalendar;
