import { useState } from "react";
import style from "./style.module.scss";
import {
  ChevronDown,
  ChevronLeft,
  ChevronRight,
  ChevronUp,
} from "lucide-react";

interface IProps {
  totalRows: number;
  perPage: number;
  currentPage: number;
  handlePageChange: (pageNumber: number) => void;
  handlePerRowsChange: (
    newPerPage?: number,
    pageNumber?: number
  ) => Promise<void>;
}

const Pagination = ({
  totalRows,
  perPage,
  currentPage,
  handlePageChange,
  handlePerRowsChange,
}: IProps) => {
  const [openDropdown, setOpenDropdown] = useState(false);

  const totalPages = Math.ceil(totalRows / perPage);

  const changePage = (page: number) => {
    if (page >= 1 && page <= totalPages) {
      handlePageChange(page);
    }
  };

  const perPageOptions = [10, 15];

  const getPageNumbers = () => {
    const pages: (number | string)[] = [];

    if (totalPages <= 5) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      if (currentPage <= 3) {
        pages.push(1, 2, 3, "...", totalPages);
      } else if (currentPage >= totalPages - 2) {
        pages.push(1, "...", totalPages - 2, totalPages - 1, totalPages);
      } else {
        pages.push(1, "...", currentPage, "...", totalPages);
      }
    }

    return pages;
  };

  return (
    <div className={style["pagination__container"]}>
      <div
        className={style["pagination__container--size"]}
        onClick={() => setOpenDropdown((prev) => !prev)}
      >
        {perPage} {openDropdown ? <ChevronUp /> : <ChevronDown />}
        {openDropdown && (
          <div className={style["pagination__container--size--dropdown"]}>
            {perPageOptions.map((option) => (
              <div
                key={option}
                className={style["pagination__container--size--dropdown--item"]}
                onClick={(e) => {
                  e.stopPropagation();
                  handlePerRowsChange(option, 1);
                  setOpenDropdown(false);
                }}
              >
                {option}
              </div>
            ))}
          </div>
        )}
      </div>

      <div className={style["pagination__container--pages"]}>
        {getPageNumbers().map((page, index) => (
          <div
            key={index}
            onClick={() => typeof page === "number" && changePage(page)}
            className={`${style["pagination__container--pages--per"]} ${
              page === currentPage
                ? style["pagination__container--pages--per--active"]
                : ""
            }`}
          >
            {page}
          </div>
        ))}
      </div>
      <div className={style["pagination__container--pages--mobile"]}>
        <div className={style["pagination__container--pages--mobile--per"]}>
          {currentPage}
        </div>
      </div>

      <div className={style["pagination__container--buttons"]}>
        <div
          className={`${style["pagination__container--buttons--per"]} ${
            currentPage === 1 ? style["disabled"] : ""
          }`}
          onClick={() => currentPage > 1 && changePage(currentPage - 1)}
        >
          <ChevronLeft />
        </div>

        <div
          className={`${style["pagination__container--buttons--per"]} ${
            currentPage === totalPages ? style["disabled"] : ""
          }`}
          onClick={() =>
            currentPage < totalPages && changePage(currentPage + 1)
          }
        >
          <ChevronRight />
        </div>
      </div>
    </div>
  );
};

export default Pagination;
